"""Trim the double original to the visible-hands duration from the saved scene."""
from pathlib import Path
import re
import wave
import json
import numpy as np

root = Path(r'D:\CWS\Zol_Project')
scene = (root/'Assets/Scenes/Battle_Proto.unity').read_text(encoding='utf-8')
block = scene.split('m_EditorClassIdentifier: Assembly-CSharp::BloodyMaryGameOverController', 1)[1].split('\n---', 1)[0]
interval = float(re.search(r'handInterval: ([\d.]+)', block).group(1))
hold = float(re.search(r'handsHoldSeconds: ([\d.]+)', block).group(1))
count = len(re.findall(r'- \{fileID:', block.split('  hands:', 1)[1].split('  retryButton:', 1)[0]))
duration = (count - 1) * interval + hold
src = root/'Assets/SoundEffect/GameOver_Hands_Original_Double.wav'
dest = root/'Assets/SoundEffect/GameOver_Hands_Final_1170ms.wav'
with wave.open(str(src), 'rb') as w:
    channels, width, rate, frames, _, _ = w.getparams()
    raw = w.readframes(frames)
assert width == 3
b = np.frombuffer(raw, np.uint8).reshape(-1, 3).astype(np.int32)
v = b[:, 0] | b[:, 1] << 8 | b[:, 2] << 16
v = np.where(v & 0x800000, v - 0x1000000, v)
original = v.reshape(-1, channels).astype(np.float64) / 8388608
n = round(duration * rate)
assert len(original) >= n
y = original[:n].copy()
fade = round(.12 * rate)
# Smooth cosine release: no abrupt gain change at the start or end of the fade.
release = .5 * (1 + np.cos(np.linspace(0, np.pi, fade)))
y[-fade:] *= release[:, None]
assert np.array_equal(y[:-fade], original[:n-fade])
assert np.all(y[-1] == 0) and np.isfinite(y).all() and np.max(np.abs(y)) < 1
pcm = np.round(y*8388608).astype(np.int32).reshape(-1)
packed = np.stack((pcm & 255, (pcm >> 8) & 255, (pcm >> 16) & 255), axis=1).astype(np.uint8)
assert not dest.exists()
with wave.open(str(dest), 'wb') as w:
    w.setnchannels(channels)
    w.setsampwidth(width)
    w.setframerate(rate)
    w.writeframes(packed.tobytes())
with wave.open(str(dest), 'rb') as w:
    assert w.getnframes() == n
print(json.dumps({'file': str(dest), 'visible_hands_seconds': duration,
    'blackout_to_red_seconds': count*interval+hold, 'hand_count': count,
    'interval': interval, 'fade_out_seconds': fade/rate,
    'duration': n/rate, 'final_sample_zero': True, 'clipping': False}, ensure_ascii=False))
