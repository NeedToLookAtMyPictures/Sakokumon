import struct, zlib, os

def make_png(width, height, pixels):
    def chunk(t, d):
        c = t + d
        return struct.pack('>I', len(d)) + c + struct.pack('>I', zlib.crc32(c) & 0xffffffff)
    raw = bytearray()
    for y in range(height):
        raw.append(0)
        for x in range(width):
            raw.extend(pixels[y * width + x])
    return (b'\x89PNG\r\n\x1a\n'
            + chunk(b'IHDR', struct.pack('>IIBBBBB', width, height, 8, 6, 0, 0, 0))
            + chunk(b'IDAT', zlib.compress(bytes(raw)))
            + chunk(b'IEND', b''))

def sprite(grid, color, cell=64, border=2):
    rows, cols = len(grid), len(grid[0])
    W, H = cols * cell, rows * cell
    r, g, b = color
    dr, dg, db = max(0, r-60), max(0, g-60), max(0, b-60)
    pixels = []
    for py in range(H):
        for px in range(W):
            gc, gr = px // cell, py // cell
            if grid[gr][gc]:
                lx, ly = px % cell, py % cell
                if lx < border or lx >= cell-border or ly < border or ly >= cell-border:
                    pixels.append([dr, dg, db, 255])
                else:
                    pixels.append([r, g, b, 255])
            else:
                pixels.append([0, 0, 0, 0])
    return make_png(W, H, pixels)

OUT = r'c:\Users\bmmah\OneDrive\Desktop\Classes\Summer 2026\3D Engines in Practice\Godot\Sakokumon\assets\sprites\Items'

T, F = True, False

items = {
    'goldBar':            ([[T, T]],                                                      (212, 175,  55)),  # gold
    'copperCoin':         ([[T]],                                                          (184, 115,  51)),  # copper
    'cross':              ([[F,T,F],[T,T,T],[F,T,F],[F,T,F]],                             (139,   0,   0)),  # dark red
    'rawSilk':            ([[T,T],[T,T]],                                                  (245, 245, 220)),  # cream
    'silkRoll_1':         ([[T,T,T,T]],                                                    (180, 100, 180)),  # purple
    'silkRoll_2':         ([[T,T,T,T]],                                                    (100, 150, 200)),  # blue
    'silkRoll_3':         ([[T,T,T,T]],                                                    (200,  80,  80)),  # crimson
    'silver':             ([[T,T,T]],                                                      (192, 192, 192)),  # silver
    'copperBar_s':        ([[T],[T]],                                                      (184, 115,  51)),  # copper
    'copperBar_m':        ([[T],[T],[T]],                                                  (184, 115,  51)),
    'copperBar_l':        ([[T],[T],[T],[T]],                                              (184, 115,  51)),
    'nagasakiTradeCoin':  ([[T]],                                                          (210, 160,  60)),  # golden copper
    'silverware':         ([[T,T]],                                                        (200, 200, 210)),  # silver-white
    'goodsSack_pepper':   ([[T,T,T],[T,T,T],[T,T,T],[T,T,T]],                             (120,  90,  60)),  # dark burlap
    'goodsSack_sugar':    ([[T,T,T],[T,T,T],[T,T,T],[T,T,T]],                             (200, 185, 145)),  # light burlap
    'camphorWood':        ([[T,T],[T,T],[T,T],[T,T]],                                      (150, 105,  60)),  # wood brown
    'ceramicBowl':        ([[T,T],[T,T]],                                                  (225, 235, 245)),  # porcelain
    'lacquerwareBowl':    ([[T,T],[T,T]],                                                  (170,  20,  20)),  # lacquer red
    'christianBook':      ([[T,T,T],[T,T,T],[T,T,T],[T,T,T]],                             ( 50,  50, 100)),  # dark blue
    'scienceBook':        ([[T,T,T],[T,T,T],[T,T,T],[T,T,T]],                             ( 40,  90,  55)),  # dark green
    'opiumSmall':         ([[T]],                                                          (220, 200, 170)),  # pale beige
    'opiumLarge':         ([[T,T],[T,T],[T,T]],                                            (200, 180, 150)),  # tan
}

for name, (grid, color) in items.items():
    data = sprite(grid, color)
    path = os.path.join(OUT, f'{name}.png')
    with open(path, 'wb') as f:
        f.write(data)
    print(f'  {name}.png  ({len(grid[0])*64}x{len(grid)*64})')

print(f'\nDone — {len(items)} sprites written to {OUT}')
