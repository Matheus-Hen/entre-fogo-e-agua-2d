"""Sintetiza a musica de fundo e os efeitos sonoros do jogo (WAV mono 16 bits).

Uso:
    python tools/audio/gerar_audio.py

Todo o audio e gerado por este script (sem amostras externas), portanto e
autoral da equipe. Saida: Between fire and water/Assets/Audio/
"""
import math
import os
import random
import struct
import wave

RATE = 22050
ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
OUT = os.path.join(ROOT, "Between fire and water", "Assets", "Audio")
random.seed(11)


def save(name, samples, gain=0.9):
    os.makedirs(OUT, exist_ok=True)
    peak = max(1e-6, max(abs(s) for s in samples))
    scale = gain / peak
    path = os.path.join(OUT, name + ".wav")
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(RATE)
        w.writeframes(b"".join(struct.pack("<h", int(max(-1, min(1, s * scale)) * 32767)) for s in samples))
    print("OK", name, round(len(samples) / RATE, 2), "s")


def env(i, n, attack=0.01, release=0.2):
    t = i / RATE
    dur = n / RATE
    a = min(1.0, t / attack) if attack > 0 else 1.0
    r = min(1.0, (dur - t) / release) if release > 0 else 1.0
    return max(0.0, a * r)


def lowpass(samples, alpha):
    out, y = [], 0.0
    for i, s in enumerate(samples):
        a = alpha(i) if callable(alpha) else alpha
        y += a * (s - y)
        out.append(y)
    return out


def tone(freq, dur, kind="sine", vol=1.0, attack=0.01, release=0.1):
    n = int(dur * RATE)
    out = []
    for i in range(n):
        ph = 2 * math.pi * freq * i / RATE
        if kind == "sine":
            v = math.sin(ph)
        elif kind == "tri":
            v = 2 / math.pi * math.asin(math.sin(ph))
        elif kind == "square":
            v = 0.6 if math.sin(ph) >= 0 else -0.6
        else:  # pluck: seno com harmonicos decaindo
            v = math.sin(ph) + 0.4 * math.sin(2 * ph) * math.exp(-i / RATE * 6) + 0.2 * math.sin(3 * ph) * math.exp(-i / RATE * 9)
        out.append(v * vol * env(i, n, attack, release))
    return out


def mix_into(buf, samples, start):
    for i, s in enumerate(samples):
        j = start + i
        if j < len(buf):
            buf[j] += s


def note_freq(semitones_from_a4):
    return 440.0 * 2 ** (semitones_from_a4 / 12)


# ------------------------------------------------------------------ musica
def musica():
    """Loop de 32 compassos curtos (~38 s) em La menor pentatonica, 100 bpm."""
    bpm = 100
    beat = 60 / bpm
    bars = 16
    total = int(bars * 4 * beat * RATE)
    buf = [0.0] * total
    # progressao Am - F - C - G (2 compassos cada, repetida)
    chords = [(-12, -9, -5), (-16, -12, -9), (-21, -17, -14), (-14, -10, -7)]
    penta = [0, 3, 5, 7, 10, 12, 15]  # La menor pentatonica
    melody_rng = random.Random(5)
    for bar in range(bars):
        chord = chords[(bar // 2) % 4]
        t0 = int(bar * 4 * beat * RATE)
        # pad
        for st in chord:
            mix_into(buf, tone(note_freq(st), 4 * beat, "tri", 0.12, 0.3, 0.6), t0)
        # baixo pulsante
        for b in range(4):
            mix_into(buf, tone(note_freq(chord[0] - 12), beat * 0.9, "sine", 0.35, 0.01, 0.2), t0 + int(b * beat * RATE))
        # tambor taiko (ruido grave) nos tempos 1 e 3, aro nos 2 e 4
        for b in range(4):
            n = int(0.25 * RATE)
            if b % 2 == 0:
                drum = [math.sin(2 * math.pi * (70 - 40 * i / n) * i / RATE) * math.exp(-i / RATE * 14) for i in range(n)]
                mix_into(buf, [d * 0.6 for d in drum], t0 + int(b * beat * RATE))
            else:
                n2 = int(0.08 * RATE)
                hat = [random.uniform(-1, 1) * math.exp(-i / RATE * 60) * 0.12 for i in range(n2)]
                mix_into(buf, hat, t0 + int(b * beat * RATE))
        # melodia (flauta) a partir do compasso 4
        if bar >= 4:
            for step in range(8):
                if melody_rng.random() < 0.7:
                    st = penta[melody_rng.randrange(len(penta))]
                    dur = beat * (0.5 if melody_rng.random() < 0.7 else 1.0)
                    mix_into(buf, tone(note_freq(st), dur, "sine", 0.18, 0.03, dur * 0.5), t0 + int(step * beat / 2 * RATE))
    # crossfade suave das bordas para o loop
    fade = int(0.05 * RATE)
    for i in range(fade):
        buf[i] *= i / fade
        buf[-1 - i] *= i / fade
    save("Musica_Base", buf, 0.7)


# ------------------------------------------------------------------ efeitos
def sfx_fogo():
    n = int(0.45 * RATE)
    noise = [random.uniform(-1, 1) for _ in range(n)]
    s = lowpass(noise, lambda i: 0.05 + 0.5 * (1 - i / n))
    s = [v * env(i, n, 0.005, 0.3) + 0.4 * math.sin(2 * math.pi * (180 - 120 * i / n) * i / RATE) * env(i, n, 0.005, 0.3) for i, v in enumerate(s)]
    save("SFX_Fogo", s)


def sfx_agua():
    n = int(0.5 * RATE)
    out = []
    for i in range(n):
        t = i / RATE
        f = 400 + 900 * (t / 0.5) + 150 * math.sin(2 * math.pi * 18 * t)
        out.append(math.sin(2 * math.pi * f * t) * env(i, n, 0.01, 0.25))
    noise = lowpass([random.uniform(-1, 1) for _ in range(n)], 0.15)
    out = [o * 0.7 + nz * 0.5 * env(i, n, 0.005, 0.4) for i, (o, nz) in enumerate(zip(out, noise))]
    save("SFX_Agua", out)


def sfx_coleta():
    buf = [0.0] * int(0.6 * RATE)
    for k, st in enumerate((3, 7, 10, 15)):
        mix_into(buf, tone(note_freq(st + 12), 0.3, "pluck", 0.6, 0.005, 0.25), int(k * 0.07 * RATE))
    save("SFX_Coleta", buf)


def sfx_dano():
    n = int(0.3 * RATE)
    s = [math.sin(2 * math.pi * (140 - 90 * i / n) * i / RATE) * math.exp(-i / RATE * 10) + random.uniform(-0.4, 0.4) * math.exp(-i / RATE * 30) for i in range(n)]
    save("SFX_Dano", s)


def sfx_inimigo():
    buf = [0.0] * int(0.7 * RATE)
    mix_into(buf, tone(note_freq(-5), 0.25, "square", 0.4, 0.005, 0.15), 0)
    mix_into(buf, tone(note_freq(-10), 0.4, "square", 0.4, 0.005, 0.3), int(0.15 * RATE))
    save("SFX_InimigoDerrotado", buf)


def sfx_apagar():
    n = int(0.9 * RATE)
    noise = [random.uniform(-1, 1) for _ in range(n)]
    hp, prev = [], 0.0
    for v in noise:  # passa-alta simples = chiado
        hp.append(v - prev)
        prev = v
    s = [v * env(i, n, 0.02, 0.7) for i, v in enumerate(hp)]
    save("SFX_Apagar", s)


def sfx_portao():
    n = int(1.6 * RATE)
    noise = lowpass([random.uniform(-1, 1) for _ in range(n)], 0.02)
    s = [v * env(i, n, 0.1, 0.6) + 0.3 * math.sin(2 * math.pi * 55 * i / RATE) * env(i, n, 0.1, 0.6) for i, v in enumerate(noise)]
    save("SFX_Portao", s)


def sfx_vitoria():
    buf = [0.0] * int(2.0 * RATE)
    for k, st in enumerate((0, 4, 7, 12)):
        mix_into(buf, tone(note_freq(st), 0.35 if k < 3 else 1.2, "tri", 0.5, 0.01, 0.2 if k < 3 else 0.9), int(k * 0.18 * RATE))
    save("SFX_Vitoria", buf)


def sfx_derrota():
    buf = [0.0] * int(2.0 * RATE)
    for k, st in enumerate((0, -2, -5, -9)):
        mix_into(buf, tone(note_freq(st - 5), 0.45 if k < 3 else 1.2, "tri", 0.5, 0.01, 0.3 if k < 3 else 0.9), int(k * 0.3 * RATE))
    save("SFX_Derrota", buf)


def sfx_pulo():
    n = int(0.18 * RATE)
    s = [math.sin(2 * math.pi * (300 + 500 * i / n) * i / RATE) * env(i, n, 0.005, 0.1) for i in range(n)]
    save("SFX_Pulo", s, 0.5)


def sfx_clique():
    save("SFX_Clique", tone(880, 0.07, "sine", 1.0, 0.002, 0.05), 0.5)


if __name__ == "__main__":
    musica()
    for fn in (sfx_fogo, sfx_agua, sfx_coleta, sfx_dano, sfx_inimigo, sfx_apagar, sfx_portao, sfx_vitoria, sfx_derrota, sfx_pulo, sfx_clique):
        fn()
