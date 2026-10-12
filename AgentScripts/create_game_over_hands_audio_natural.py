"""Preserve the complete recording using correlated overlap-add and gentle rhythm shaping."""
from pathlib import Path
import wave
import json
import numpy as np

src = Path(r'C:\Users\우성\Downloads\monitor_hands_dark.wav')
dest = Path(r'D:\CWS\Zol_Project\Assets\SoundEffect\GameOver_Hands_Natural.wav')
with wave.open(str(src), 'rb') as w:
    channels, width, sr, frames, _, _ = w.getparams()
    raw = w.readframes(frames)
assert width == 3
b = np.frombuffer(raw, np.uint8).reshape(-1, 3).astype(np.int32)
v = b[:, 0] | b[:, 1] << 8 | b[:, 2] << 16
v = np.where(v & 0x800000, v - 0x1000000, v)
x = v.reshape(-1, channels).astype(np.float64) / 8388608
x -= np.mean(x, axis=0)

# Keep the whole source's changing impacts, low resonance and dark tail.
# Correlated overlaps retain local pitch and avoid repeating one isolated knock.
body_seconds = 1.17
length = round(body_seconds * sr)
window_size = round(.032 * sr)
hop = round(.008 * sr)
search = round(.004 * sr)
window = np.hanning(window_size)
output = np.zeros((length + window_size, channels))
weights = np.zeros(length + window_size)
padded = np.pad(x, ((window_size, window_size), (0, 0)), mode='reflect')

def source_position(time):
    if time <= .84:
        return time * .70 / .84
    return .70 + (time - .84) * .30 / .33

for target in range(0, length, hop):
    expected = round(source_position(target / sr) * sr) + window_size
    expected = min(max(0, expected), len(padded) - window_size)
    best = expected
    overlap = window_size - hop
    if target:
        known_weights = weights[target:target + overlap]
        reference = output[target:target + overlap, 0] / np.maximum(known_weights, 1e-10)
        reference -= np.mean(reference)
        ref_norm = np.linalg.norm(reference)
        score = -np.inf
        for candidate in range(max(0, expected-search), min(len(padded)-window_size, expected+search)+1, 16):
            segment = padded[candidate:candidate+overlap, 0]
            segment = segment - np.mean(segment)
            similarity = np.dot(reference, segment) / (ref_norm*np.linalg.norm(segment)+1e-10)
            similarity -= .025 * abs(candidate-expected) / search
            if similarity > score:
                score, best = similarity, candidate
    output[target:target+window_size] += padded[best:best+window_size] * window[:, None]
    weights[target:target+window_size] += window
body = output[:length] / np.maximum(weights[:length, None], 1e-10)

# Broad accents shape one continuous rush of sound instead of cutting it into short pulses.
t = np.arange(length) / sr
rhythm = np.zeros(length)
strengths = [.80, .91, .86, .95, .82, .99, .90, .87, 1., .93, .89, 1.]
for i, strength in enumerate(strengths):
    rhythm += strength * np.exp(-.5 * ((t-i*.07)/.018)**2)
gain = .82 + .24 * rhythm
body *= gain[:, None]
# A very quiet early reflection adds cohesion without replacing the source's character.
delay = round(.013 * sr)
dry = body.copy()
body[delay:] += dry[:-delay] * .07
fade_in = round(.0015 * sr)
fade_out = round(.06 * sr)
body[:fade_in] *= np.linspace(0, 1, fade_in)[:, None]
body[-fade_out:] *= np.linspace(1, 0, fade_out)[:, None]
y = np.concatenate((np.zeros((round(.07*sr), channels)), body))
y *= 10**(-3/20) / np.max(np.abs(y))
assert np.isfinite(y).all() and np.max(np.abs(y)) < 1
assert len(y) == round(1.24*sr)
pcm = np.round(y*8388607).astype(np.int32).reshape(-1)
packed = np.stack((pcm & 255, (pcm >> 8) & 255, (pcm >> 16) & 255), axis=1).astype(np.uint8)
assert not dest.exists()
with wave.open(str(dest), 'wb') as w:
    w.setnchannels(channels)
    w.setsampwidth(3)
    w.setframerate(sr)
    w.writeframes(packed.tobytes())
with wave.open(str(dest), 'rb') as w:
    assert w.getnframes() == len(y)
print(json.dumps({'file': str(dest), 'duration': len(y)/sr, 'sample_rate': sr,
    'bits': 24, 'source': 'complete original recording', 'rhythm_accents': 12,
    'interval': .07, 'peak_dbfs': -3}, ensure_ascii=False))
