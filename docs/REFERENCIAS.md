# Referências e créditos

## Narrativa

- *Entre fogo e água*: manuscrito fornecido pelo grupo (`entre-fogo-e-agua.pdf`, 21 páginas, fora do repositório). Autoria e edição ainda não informadas; **confirmar com a equipe antes da apresentação**. O jogo usa personagens (Magnus, Mark), a ASDA/DASA e o conflito entre fogo e água. As regras de jogo são adaptações da equipe.

## Projeto-base e adaptações

- Versão 2D da própria equipe (*Between fire and water*, RPG por turnos), commit `d073241`.
- Projeto Plataforma3D e aulas da disciplina, como referência de controle 3D, coleta, física e fluxo de cenas.
- Adaptações da equipe: afinidade elemental (guardas absorvem o próprio elemento; água reforça o gelo), energia por lançamento, quebra-cabeça da placa e da ponte, queda no rio com checkpoint e pontuação por eventos únicos.

## Modelos 3D

| Asset | Origem | Arquivo-fonte |
| --- | --- | --- |
| Magnus (malha, rig e clips Idle/Walk/Cast) | Gerado por script Python no Blender 5.3 a partir do model sheet da equipe (`Art/ModelSheets/Magnus/`) | `tools/blender/gerar_magnus.py` → `Art/Modelos3D/Magnus.blend` |
| Soldado ASDA (guardas) | Model sheet da equipe em Blender; exportado com modificadores aplicados e origem nos pés | `Art/ModelSheets/Soldado-ASDA.blend` (autoria a confirmar) + `tools/blender/exportar_soldado.py` |
| Caixa, Pista, Portão, Barraca, Braseiro, Torre | Gerados por script no Blender | `tools/blender/gerar_props.py` → `Art/Modelos3D/Props.blend` |
| Chão, muros, rio, ponte, placa, gelo | Primitivas da Unity com materiais próprios | `ConstrutorJogo3D.cs` |

## Áudio

A música `Musica_Base` e os 11 efeitos `SFX_*` foram sintetizados pelo script `tools/audio/gerar_audio.py`, sem amostras externas. Os arquivos estão em `Between fire and water/Assets/Audio/`.

## Recursos embutidos da Unity

Fonte `LegacyRuntime.ttf`, textura `Default-Particle`, material `Default-Skybox` e shaders Standard, Particles/Additive e Sprites/Default.

## Uso de IA (registro exigido pelo guia)

Nesta etapa (19/11/2026), a conversão para 3D foi feita com apoio do assistente de IA Claude (Anthropic): scripts C#, gerador de cenas do Editor, scripts Blender de modelagem e animação, síntese de áudio, testes e documentação. A equipe deve **revisar, jogar e conseguir explicar** cada script antes da apresentação, e registrar aqui o que alterar.

## Acervo 2D anterior

Concepts, model sheets, fundos e sketches continuam em `Art/` e `LevelDesign/`, com inventários em `Art/inventario.json` e `imagens/inventario.json`. Autoria, ferramenta e licença dessas imagens não foram informadas.
