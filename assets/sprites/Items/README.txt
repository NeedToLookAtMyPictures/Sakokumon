All files in this folder are placeholders copied from copperCoin.png.
Replace each file with the correct sprite when art is ready.
Each sprite should be sized to fit a 64x64px grid cell (e.g. a 2x3 item = 128x192px).

CHANGES FROM PRIOR DATA:
  goldBar     - Grid corrected to [[true, true]] (2x1 per spec); legalEndYear corrected to 1762 (banned 1763)
  copperCoin  - legalStartYear/legalEndYear set to 9999/9999 (always illegal — regular coins were never for trade)

-----------------------------------------------------------------------
KEY               | NAME                  | GRID  | LEGAL WINDOW  | NOTES
-----------------------------------------------------------------------
goldBar           | Gold                  | 2x1   | until 1762    | banned 1763
copperCoin        | Copper Coin           | 1x1   | never legal   | regular coins always forbidden for trade
cross             | Christian Cross       | 3x4*  | never legal   | non-rectangular, see data.json for bitmap
rawSilk           | Raw Silk              | 2x2   | always legal  |
silkRoll_1/2/3    | Silk Roll             | 4x1   | always legal  | 3 color/pattern variants
silver            | Silver                | 3x1   | until 1667    | banned 1668
copperBar_s       | Copper Bar (Small)    | 1x2   | always legal  |
copperBar_m       | Copper Bar (Medium)   | 1x3   | always legal  |
copperBar_l       | Copper Bar (Large)    | 1x4   | always legal  |
nagasakiTradeCoin | Nagasaki Trade Coin   | 1x1   | 1659-1685     | only appears in game during that window
silverware        | Silverware            | 2x1   | until 1680    | silver export loophole, banned ~1680
goodsSack_pepper  | Goods Sack (Pepper)   | 3x4   | always legal  | kanji for pepper on sack
goodsSack_sugar   | Goods Sack (Sugar)    | 3x4   | always legal  | kanji for sugar on sack
camphorWood       | Camphor Wood          | 2x4   | always legal  |
ceramicBowl       | Ceramic Bowl          | 2x2   | always legal  |
lacquerwareBowl   | Lacquerware Bowl      | 2x2   | always legal  |
christianBook     | Christian Text        | 3x4   | never legal   | cross on cover
scienceBook       | Scientific Text       | 3x4   | from 1720     | illegal 1685-1719
opiumSmall        | Opium (Medical)       | 1x1   | until 1856    | banned 1857
opiumLarge        | Opium (Bulk)          | 2x3   | never legal   | always smuggling
-----------------------------------------------------------------------
