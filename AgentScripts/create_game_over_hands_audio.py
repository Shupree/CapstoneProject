"""Offline editing of the supplied recording; does not run Unity or gameplay."""
from pathlib import Path
import json
import wave
import numpy as np

SOURCE = Path(r"C:\Users\우성\Downloads\monitor_hands_dark.wav")
OUTPUT = Path(r"D:\CWS\Zol_Project\Assets\SoundEffect\GameOver_Hands_12Hits_70ms.wav")

with wave.open(str(SOURCE), "rb") as recording:
    channels, width, rate, frames, _, _ = recording.getparams()
    raw = recording.readframes(frames)
assert width == 3, "Expected the supplied 24-bit PCM recording"
bytes24 = np.frombuffer(raw, dtype=np.uint8).reshape(-1, 3).astype(np.int32)
pcm = bytes24[:, 0] | (bytes24[:, 1] << 8) | (bytes24[:, 2] << 16)
pcm = np.where(pcm & 0x800000, pcm - 0x1000000, pcm)
source = pcm.reshape(-1, channels).astype(np.float64) / 8388608.0
source -= np.mean(source, axis=0)

interval = 0.07
count = 12
hold = 0.4
duration = count * interval + hold
output = np.zeros((round(duration * rate), channels), dtype=np.float64)
hit_length = round(0.056 * rate)
attack = round(0.001 * rate)
release = round(0.012 * rate)
# Use the first isolated impact's attack, avoiding later knocks in the original recording.
core = source[round(0.003 * rate):round(0.029 * rate)]
tail = source[round(0.105 * rate):round(0.135 * rate)]
hit = np.concatenate((core, tail), axis=0)[:hit_length].copy()
blend = round(0.003 * rate)
join = len(core)
hit[join - blend:join] *= np.linspace(1, 0, blend)[:, None]
hit[join:join + blend] *= np.linspace(0, 1, blend)[:, None]
envelope = np.exp(-np.arange(len(hit)) / (rate * 0.029))
envelope[:attack] *= np.linspace(0, 1, attack)
envelope[-release:] *= np.linspace(1, 0, release)
hit *= envelope[:, None]
hit /= max(np.max(np.abs(hit)), 1e-12)
strengths = [0.80, 0.88, 0.83, 0.92, 0.86, 0.95, 0.89, 0.97, 0.91, 1.0, 0.94, 1.0]
onsets = []
for index, strength in enumerate(strengths):
    start = round((index + 1) * interval * rate)
    onsets.append(start / rate)
    output[start:start + len(hit)] += hit * strength

# Retain a quiet piece of the original dark tail after the twelfth attack.
start = round(onsets[-1] * rate) + len(hit)
ambient = source[round(0.72 * rate):round(0.97 * rate)].copy()
ambient *= np.linspace(1, 0, len(ambient))[:, None] ** 2
ambient[:round(0.006 * rate)] *= np.linspace(0, 1, round(0.006 * rate))[:, None]
output[start:start + len(ambient)] += ambient * 0.75
output *= (10 ** (-3 / 20)) / max(np.max(np.abs(output)), 1e-12)
assert np.all(np.isfinite(output)) and np.max(np.abs(output)) < 1
assert len(onsets) == 12 and np.allclose(np.diff(onsets), interval)
pcm_out = np.round(output * 8388607).astype(np.int32).reshape(-1)
packed = np.empty((len(pcm_out), 3), dtype=np.uint8)
packed[:, 0] = pcm_out & 255
packed[:, 1] = (pcm_out >> 8) & 255
packed[:, 2] = (pcm_out >> 16) & 255
assert not OUTPUT.exists(), "Preserve any existing output"
with wave.open(str(OUTPUT), "wb") as recording:
    recording.setnchannels(channels)
    recording.setsampwidth(3)
    recording.setframerate(rate)
    recording.writeframes(packed.tobytes())
with wave.open(str(OUTPUT), "rb") as recording:
    assert recording.getnframes() == len(output)
    assert recording.getsampwidth() == 3
print(json.dumps({"output": str(OUTPUT), "duration_seconds": duration, "hits": len(onsets),
    "onsets_seconds": onsets, "sample_rate": rate, "bits": 24,
    "peak_dbfs": round(20 * np.log10(np.max(np.abs(output))), 2)}, ensure_ascii=False))
