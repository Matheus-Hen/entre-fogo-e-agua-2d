# Como abrir e jogar (versão 3D)

1. No Unity Hub, adicione a pasta `Between fire and water/` e abra com **Unity 6000.6.0f1**.
2. Abra `Assets/Scenes/Menu.unity` e pressione **Play**. O Build Settings já lista `Menu` (0) e `Fase01_BaseASDA` (1).
3. No menu, clique em **Jogar**. **Como jogar** mostra as regras.

## Controles

WASD ou setas: mover • Shift: correr • Espaço: pular • Mouse (ou J/L): câmera • Clique esquerdo ou F: lançar o elemento • Q, Tab ou roda do mouse: trocar Fogo ↔ Água • Esc: pausa (Continuar, Reiniciar, Menu).

## Percurso para demonstração (cerca de 5 minutos)

1. **Acampamento:** empurre a caixa à esquerda da trilha até a **placa dourada**, e a ponte sobe. Troque para **Água** (Q) e apague a barreira de fogo do nicho leste (2 acertos). Pegue a **Pista 1**.
2. **Rio:** atravesse pela ponte. Pular no rio mostra o dano e o retorno ao checkpoint.
3. **Base:** o guarda de aura **laranja** só sofre dano de **Água**, e o de aura **ciana** só de **Fogo**. Acertar com o elemento errado cura o guarda. No nicho oeste, derreta o **gelo com Fogo** (Água o reforça) e pegue a **Pista 2**.
4. **Muro interno:** apague a segunda barreira com Água (3 acertos).
5. **Pátio norte:** derrote o **guarda elite** (aura ciana, use Fogo). Ele deixa cair a **Pista 3**.
6. Com 3/3 pistas o **portão abre**. Atravesse para ver a **VITÓRIA**, depois use **Jogar novamente**.
7. Para mostrar a **DERROTA**, fique parado perto dos guardas até a vida zerar.

## Testes automatizados

Window > General > Test Runner > **PlayMode** > Run All. São 12 testes em `Assets/Tests/PlayMode/FaseTests.cs`, todos aprovados em 19/11/2026.

## Regenerar o conteúdo (opcional)

As cenas, prefabs, materiais e Animator Controllers foram gerados pelo menu **Entre Fogo e Água > Gerar jogo 3D** (`Assets/scripts/Jogo3D/Editor/ConstrutorJogo3D.cs`). Rodar de novo **sobrescreve** `Menu.unity`, `Fase01_BaseASDA.unity`, os prefabs e os controllers. Ajustes feitos à mão nesses assets se perdem.

As fontes de arte e áudio são regeneradas fora da Unity:

```powershell
& "C:\Program Files (x86)\Steam\steamapps\common\Blender\blender.exe" -b --factory-startup --python tools/blender/gerar_magnus.py
```

```powershell
& "C:\Program Files (x86)\Steam\steamapps\common\Blender\blender.exe" -b --factory-startup --python tools/blender/gerar_props.py
```

```powershell
& "C:\Program Files (x86)\Steam\steamapps\common\Blender\blender.exe" -b Art/ModelSheets/Soldado-ASDA.blend --python tools/blender/exportar_soldado.py
```

```powershell
python tools/audio/gerar_audio.py
```

## Build (recomendada, não obrigatória)

File > Build Profiles > Windows > Build, em uma pasta limpa (ex.: `Builds/Windows/`). Preserve a pasta inteira gerada, não apenas o `.exe`.
