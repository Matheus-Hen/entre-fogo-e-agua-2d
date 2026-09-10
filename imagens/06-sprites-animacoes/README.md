# Planejamento dos sprites e animações

As pranchas ficam em [model sheet](../02-model-sheet/) e [concept art](../01-concept-art/); não são duplicadas nesta pasta.

| Recurso | Frames propostos | FPS | Loop |
| --- | --- | --- | --- |
| Magnus parado | 1 por direção, 4 total | estático | sim |
| Magnus andando | 4 por direção, 16 total | 8 | sim |
| Ataque / fogo / água | 4 cada, 12 total | 10 | não |
| Defesa | 2 | 6 | não |
| Dano | 2 | 8 | não |
| Derrota | 4 | 6 | não |
| Efeito fogo / água | 6 cada | 12 | não |

Alvos iniciais: PNG RGBA, tile 64 x 64 px, personagem 128 x 192 px, 64 PPU, pivô nos pés. Corrigir fundos, alinhamento e perspectiva antes de exportar. Planejar NPCs e inimigos conforme o [GDD](../../docs/GDD-v0.2.md#sprites-e-animações---plano-técnico). Nenhuma animação está implementada.
