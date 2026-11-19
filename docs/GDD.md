# Entre Fogo e Água 3D | GDD (documento vivo)

Projeto de Jogos Digitais II, V2: jogo 3D em Unity e C#. Atualizado em 19/11/2026 (versão de organização v0.2, trecho integrado). A versão 2D anterior (RPG por turnos) está preservada no histórico do Git (commit `d073241`, "Finalize projeto v1.0").

## 1. Identificação

| Item | Definição |
| --- | --- |
| Nome | Entre Fogo e Água: Infiltração na Base ASDA |
| Gênero | Ação e aventura 3D em terceira pessoa, com quebra-cabeça físico |
| Plataforma-alvo | PC (Windows), teclado e mouse |
| Motor | Unity 6.6 (6000.6.0f1), C#, Built-in Render Pipeline, Input System |
| Público | 13+ (fantasia juvenil, violência estilizada sem sangue) |
| Duração de uma partida | 5 a 10 minutos |
| Integrantes | Larissa Campos Cardoso, Izak Pinheiro, Matheus Henrique |

## 2. Conceito

Magnus Lubros, um garoto que controla fogo e água, descobre que a ASDA (agência que dizia protegê-lo) quer eliminá-lo. Ele foge do acampamento, atravessa o rio e invade a base para reunir três provas contra a organização. A fase é curta, com começo, desafio, vitória, derrota e reinício, como pede o escopo da disciplina.

**Referências.** Narrativa: o manuscrito *Entre fogo e água* (ver [REFERENCIAS.md](REFERENCIAS.md)). Jogo: a versão 2D da própria equipe (exploração e combate por turnos com fogo e água) e o projeto-base Plataforma3D das aulas (controle 3D, coleta, física e cenas).

## 3. Objetivo, vitória e derrota

- **Objetivo:** coletar as 3 pistas da ASDA e atravessar o portão da base.
- **Vitória:** com as 3 pistas, o portão se abre (Animator). Entrar na saída depois do portão mostra a tela **VITÓRIA!** com pontuação, pistas e tempo.
- **Derrota:** a vida de Magnus chega a 0 por disparos dos guardas, contato com guardas, chamas ou quedas no rio. Aparece a tela **DERROTA** com o motivo.
- **Nova tentativa:** "Jogar novamente" recarrega a cena e restaura posição, vida, energia, pontuação, pistas, guardas, barreiras, caixas e ponte. O menu de pausa (Esc) também permite reiniciar.

## 4. Controles (também exibidos no jogo)

| Ação | Tecla |
| --- | --- |
| Mover (relativo à câmera) | WASD ou setas |
| Correr | Shift |
| Pular | Espaço |
| Girar a câmera | Mouse (ou J / L) |
| Lançar o elemento ativo | Clique esquerdo ou F |
| Trocar Fogo ↔ Água | Q, Tab ou roda do mouse (1 = Fogo, 2 = Água) |
| Pausa | Esc |

## 5. Core loop

Explorar → identificar o obstáculo e seu elemento → escolher o elemento certo ou usar a física → superar (barreira, guarda, ponte) → ganhar pontos ou pista → avançar até o portão.

## 6. Mecânicas

**Mecânica principal: lançamento elemental.** Magnus lança projéteis com Rigidbody e gravidade (trajetória em arco). Cada lançamento gasta 12 de energia (máximo 100, recarga de 14/s), o que obriga a escolher bem quando atirar.

**Regra criada pela equipe: afinidade elemental.** É a adaptação em relação ao projeto-base e à versão 2D.

- Guardas de **fogo** (aura laranja) **absorvem** fogo e recuperam metade do dano; só sofrem dano de **água**. Guardas de **água** (aura ciana) funcionam ao contrário.
- **Barreira de fogo:** bloqueia a passagem e queima ao toque. Só é apagada com **água** (2 ou 3 acertos); fogo não tem efeito.
- **Barreira de gelo:** só derrete com **fogo** (3 acertos). **Água reforça o gelo** (+1 acerto necessário, até 6).

Com isso, cada alvo exige uma decisão de elemento, e errar custa energia, tempo e até cura o inimigo.

**Física (Rigidbody).**

- Caixas empurráveis (Rigidbody, 15 kg). Magnus empurra ao encostar.
- A explosão da bola de fogo também empurra caixas próximas (`AddExplosionForce`).
- Os projéteis são corpos físicos com gravidade.

**Interações que mudam o estado do jogo.**

- Coletar pistas (trigger).
- Caixa sobre a placa de pressão, que ergue a ponte.
- Apagar ou derreter barreiras.
- Derrotar guardas.
- Abrir o portão.
- Checkpoint.
- Queda no rio: −30 de vida e volta ao último ponto seguro.

**Inimigos.** Os guardas patrulham entre dois pontos. Ao ver Magnus (alcance de 14 m, com linha de visão), viram-se para ele e disparam a cada 2,6 s (10 de dano). O guarda elite tem 120 de vida e deixa cair a pista 3.

## 7. Pontuação

A pontuação é calculada em `GerenciadorPartida.RegistrarPontos`. Cada evento tem um id único guardado em um `HashSet`, então o mesmo evento nunca conta duas vezes.

| Evento | Pontos |
| --- | --- |
| Pista coletada | +100 |
| Guarda derrotado | +50 (elite +100) |
| Barreira apagada ou derretida | +25 |
| Placa ativada (ponte) | +25 |
| Bônus de tempo na vitória | 300 − segundos (mínimo 0) |

## 8. Fase e desafios

**Fluxo de cenas:** `Menu` (botão Jogar) → `Fase01_BaseASDA` → Vitória ou Derrota → Jogar novamente ou Menu.

**Mapa da fase (de sul para norte, ~35 × 105 m):**

1. **Acampamento (desafio 1, física + água).** Ponto inicial, barracas e fogueira. A ponte está submersa: é preciso **empurrar uma caixa até a placa dourada** para erguê-la. No nicho leste, a **Pista 1** está atrás de uma barreira de fogo, que só a água apaga.
2. **Rio.** Cair custa vida e volta ao checkpoint. As caixas que caem no rio voltam ao lugar original, para o quebra-cabeça nunca ficar impossível.
3. **Pátio da base (desafio 2, combate com afinidade).** Dois guardas, um de fogo e um de água. No nicho oeste, a **Pista 2** está atrás de gelo, que só o fogo derrete e a água reforça.
4. **Muro interno.** Segunda barreira de fogo, que exige 3 acertos de água, enquanto os guardas atiram.
5. **Pátio norte.** Um guarda de fogo e o **guarda elite de água**, que deixa cair a **Pista 3**.
6. **Portão da base.** Abre com as 3 pistas. A saída depois dele dá a vitória.

## 9. Direção visual e sonora

- **Visual:** low-poly com cores funcionais. Laranja indica fogo e ciano indica água (auras, projéteis, HUD). O dourado marca o que é interativo (placa, pistas). A ASDA usa azul-grafite e o acampamento usa verde e terra. Magnus segue o model sheet (jaqueta azul-marinho com listras laranja e ciano).
- **Câmera:** terceira pessoa com órbita pelo mouse e anticolisão com paredes (SphereCast).
- **Áudio:** música original em loop (Lá menor pentatônica, 100 bpm, taiko e flauta sintetizados) e 11 efeitos ligados a eventos: lançar fogo, lançar água, coleta, dano, guarda derrotado, apagar ou derreter, portão, pulo, clique, vitória e derrota.

## 10. Animação

| Clip | Origem | Onde é usado |
| --- | --- | --- |
| Magnus_Idle, Magnus_Walk, Magnus_Cast | Blender (rig próprio, FBX) | Animator `Magnus.controller`: parâmetros Velocidade (float) e Lancar (trigger) |
| Guarda_Patrulha, Guarda_Atingido | Unity (.anim) | Animator `Guarda.controller`: trigger Atingido |
| Pista_Flutuar | Unity (.anim) | Animator `Pista.controller` |
| Portao_Fechado, Portao_Abrindo | Unity (.anim) | Animator `Portao.controller`: bool Aberto |

## 11. Requisitos da disciplina → onde conferir

| Requisito | Onde está |
| --- | --- |
| Controle do personagem (C#) | `scripts/Jogo3D/MagnusMovimento.cs` (`Update`, `LerEntrada`) |
| Mecânica principal | `MagnusPoderes.Lancar`, `ProjetilElemental.OnCollisionEnter` |
| Interação que muda estado | `Pista.OnTriggerEnter`, `PlacaPressao.OnTriggerEnter`, `BarreiraFogo.ReceberElemento`, `PortaoBase.Verificar` |
| Pontuação sem duplicar | `GerenciadorPartida.RegistrarPontos` / `ColetarPista` |
| Colisores 3D | Box (chão, muros, caixas, barreiras), Capsule (guardas), Sphere (projéteis, pistas), CharacterController (Magnus) |
| Rigidbody com efeito | Caixas (`CaixaEmpurravel`, empurrão em `MagnusMovimento.OnControllerColliderHit`), projéteis, explosão de fogo |
| Prefab com 2+ instâncias | `Prefabs/Guarda` (4), `Caixa` (4), `Pista` (2 + 1 criada em jogo), `BarreiraFogo` (2), projéteis criados em jogo |
| Animação | `Assets/Animations/*.anim` e `*.controller` |
| Música e efeito sonoro | `GerenciadorAudio`; `Assets/Audio/` |
| 2 cenas e transição | `Menu` → `Fase01_BaseASDA` (`MenuPrincipal.BotaoJogar`) |
| Menu, HUD e indicadores | `MenuPrincipal`, `HudPartida` (pontos, pistas, tempo, vida, energia, elemento, objetivo) |
| Vitória, derrota e reinício | `GerenciadorPartida.Vencer` / `Perder` / `Reiniciar`; `SaidaFase`; `MagnusStatus.ReceberDano` |
| Duas situações de desafio | Acampamento (caixa, placa e barreira de fogo) e Base (guardas com afinidade, gelo, 2ª barreira) |

## 12. Responsabilidades (proposta)

| Área | Responsável |
| --- | --- |
| Design + produção (GDD, escopo, balanceamento) | Larissa Campos Cardoso |
| Arte 3D + animação, level + áudio (modelos Blender, clips, mapa, sons) | Izak Pinheiro |
| Programação + QA (C#, física, UI, cenas, testes) | Matheus Henrique |

Cada integrante deve revisar e conseguir explicar os scripts da sua área (a apresentação final inclui perguntas sobre o código).

## 13. Testes

Testes automatizados de Play Mode em `Assets/Tests/PlayMode/FaseTests.cs` (Window > General > Test Runner > PlayMode). Em 19/11/2026: **12 de 12 aprovados**. Eles cobrem:

- elementos obrigatórios presentes na cena;
- pontuação sem duplicar;
- coleta da pista por trigger;
- afinidade dos guardas;
- projétil físico ferindo o guarda;
- barreira de fogo e gelo;
- caixa na placa erguendo a ponte;
- queda no rio com retorno ao checkpoint;
- derrota seguida de reinício que restaura o estado;
- 3 pistas abrindo o portão e dando vitória, com reinício após a vitória;
- botão Jogar do menu carregando a fase.

**Pendências de QA:** partida completa jogada manualmente do início ao fim, playtest com outra pessoa e build de teste (recomendada, não obrigatória).

## 14. Decisões e alterações

| Data | Decisão | Efeito no jogo |
| --- | --- | --- |
| 08/10/2026 | Fim da versão 2D (RPG por turnos, três mapas). | Base de narrativa, personagens e regras de fogo e água. |
| 19/11/2026 | Conversão para 3D em uma fase curta (exigência da V2). | Três mapas 2D condensados em uma fase com acampamento, rio e base. Combate por turnos trocado por ação em tempo real. |
| 19/11/2026 | Regra de afinidade elemental (absorve o próprio elemento, água reforça gelo). | Cada alvo exige escolher o elemento, e errar tem custo. |
| 19/11/2026 | Energia por lançamento em vez de turnos. | Mantém a gestão de recurso da versão 2D. |
| 19/11/2026 | XP e níveis removidos; pontuação por eventos únicos. | Atende à pontuação exigida e simplifica o escopo. |
| 19/11/2026 | Queda no rio custa 30 de vida e volta ao checkpoint; caixas voltam ao lugar. | Evita bloqueio de progressão. |
| 19/11/2026 | Render pipeline Built-in (já ativo no projeto) e UI com Canvas e Text (legado). | Evita trocar o pipeline perto da entrega. |

## 15. Instruções de execução

Ver [COMO-ABRIR-PROTOTIPO.md](COMO-ABRIR-PROTOTIPO.md). Resumo: abrir `Between fire and water/` no Unity 6000.6.0f1, abrir `Assets/Scenes/Menu.unity` e pressionar Play.

## 16. Créditos e fontes

Ver [REFERENCIAS.md](REFERENCIAS.md).
