"""Join two original recordings, trimming only the quiet join tail and crossfading 8ms."""
from pathlib import Path
import json
import wave
import numpy as np

source = Path(r'C:\Users\우성\Downloads\monitor_hands_dark.wav')
destination = Path(r'D:\CWS\Zol_Project\Assets\SoundEffect\GameOver_Hands_Original_Double.wav')
with wave.open(str(source), 'rb') as w:
    channels, width, rate, frames, _, _ = w.getparams()
    raw = w.readframes(frames)
assert width == 3
b = np.frombuffer(raw, np.uint8).reshape(-1, 3).astype(np.int32)
v = b[:, 0] | b[:, 1] << 8 | b[:, 2] << 16
v = np.where(v & 0x800000, v - 0x1000000, v)
x = v.reshape(-1, channels).astype(np.float64) / 8388608
hop = round(.002 * rate)
mono_energy = np.mean(x*x, axis=1)
energy = np.sqrt(np.mean(mono_energy[:len(x)//hop*hop].reshape(-1, hop), axis=1))
energy = np.convolve(energy, np.ones(5)/5, mode='same')
# Preserve every original attack; trim the quiet fade after the last substantial impact
# only from the first copy. The second copy keeps its complete ending.
active = np.flatnonzero(energy > np.max(energy) * .06)
assert len(active)
cut = min(len(x), (int(active[-1])+1)*hop + round(.025*rate))
fade = round(.008 * rate)
assert cut > fade
first = x[:cut]
f = np.linspace(0, 1, fade)[:, None]
join = first[-fade:]*(1-f) + x[:fade]*f
joined = np.concatenate((first[:-fade], join, x[fade:]))
assert np.array_equal(joined[:cut-fade], x[:cut-fade])
assert np.array_equal(joined[cut:], x[fade:])
assert np.isfinite(joined).all() and np.max(np.abs(joined)) < 1
pcm = np.round(joined*8388608).astype(np.int32).reshape(-1)
packed = np.stack((pcm&255, (pcm>>8)&255, (pcm>>16)&255), axis=1).astype(np.uint8)
assert not destination.exists(), 'Preserve existing audio'
with wave.open(str(destination), 'wb') as w:
    w.setnchannels(channels)
    w.setsampwidth(width)
    w.setframerate(rate)
    w.writeframes(packed.tobytes())
with wave.open(str(destination), 'rb') as w:
    assert w.getnframes() == len(joined)
print(json.dumps({'file': str(destination), 'duration': len(joined)/rate,
    'first_copy_end': cut/rate, 'trimmed_join_tail': (len(x)-cut)/rate,
    'crossfade_ms': 1000*fade/rate, 'rate': rate, 'bits': width*8,
    'speed_and_pitch_changed': False, 'clipping': False}, ensure_ascii=False))
