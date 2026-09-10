"""Gera a revisão consolidada após o recebimento das artes, sem alterar v0.1/v0.2."""
import json
from pathlib import Path
from xml.sax.saxutils import escape
from reportlab.platypus import SimpleDocTemplate, Paragraph, PageBreak, Image, Spacer
from reportlab.lib.styles import ParagraphStyle
from reportlab.lib.colors import HexColor
from PIL import Image as PILImage
from gerar_gdd import PAGES as INITIAL
from gerar_guia_visual import PAGES as VISUAL

ROOT = Path(__file__).resolve().parents[1]
VERSION = 'v0.2.1'
REPLACEMENTS = {
 'Entrega e versão': 'Entrega 2 - Direção Visual e Level Design. Revisão documental v0.2.1, preparada em 10/09/2026 após receber e organizar 29 imagens. Consolida concepção, identidade visual, material de referência e planejamento das fases. Ainda não há projeto Unity implementado ou build jogável.',
 'Status das decisões': 'Nome, gênero, integrantes, público, plataformas e mecânicas básicas foram informados pelo grupo. O acervo visual foi fornecido pelo usuário e está organizado no repositório. Papéis, escopo, controles, regras e valores de produção continuam como propostas iniciais; recebimento de imagens não equivale à validação técnica ou aprovação final de todos os designs.',
 'Prioridades imediatas': 'Validar os papéis, revisar consistência das artes e preparar os sprites a partir das referências recebidas. Implementar na Unity exploração conectada a combate e verificar cedo os controles em Windows e Android. O guia de prompts v0.2 permanece como histórico de produção.',
 'Conteúdo desta entrega': 'GDD consolidado v0.2.1 em Markdown e PDF, 29 imagens organizadas em Art e LevelDesign, índice de acervo e inventário técnico com dimensões, origem e SHA-256. Há concepts, model sheets, retratos, fundos, sketches e estudos de animação. Não há sprites finais recortados, animações integradas ou build.',
 'Versionamento e envio': 'O mesmo repositório local contém a concepção v0.1, o guia de planejamento v0.2 e as artes recebidas posteriormente. Esta revisão é v0.2.1; a tag v0.2 deve permanecer preservada. Falta criar e disponibilizar o repositório remoto para acesso do professor.',
 'Pendências da equipe': 'Validar responsabilidades; confirmar autoria e créditos do livro e das imagens, ferramenta de produção e condições de uso; revisar as pranchas; preparar sprites e tiles; implementar o protótipo; criar o repositório remoto. Não atribuir a um integrante a produção de uma imagem sem confirmação.',
 'Histórico': 'v0.1 - concepção inicial. v0.2 - direção visual, prompts e planejamento de fases. Recebimento posterior - organização de 29 imagens, registrada no commit a3f6945. v0.2.1 - consolidação deste GDD com estado real do acervo, referências visuais e decisões de integração. Revisões documentais realizadas em 10/09/2026.',
 'Mapa 1 - Acampamento e templo': 'Entrada > pátio com B1 treino > templo com P1 traição > retorno ao pátio > trilha com B2 guarda > saída. A trilha é acessível pelo pátio, como no sketch, mas só é liberada após P1. O diagrama anotado representa a ordem dos objetivos; sua ligação direta templo-trilha não define uma passagem física. Total: dois encontros.',
 'Mapa 2 - Posto DASA': 'Recepção > corredor com B3 guarda > arquivo com P2 identificação > sala de segurança com B4 guarda e terminal com P3 localização da base > saída. As duas pistas e vitórias são obrigatórias; permitir retorno às salas. DASA é a empresa de fachada ligada à ASDA. Total: dois encontros.',
 'Mapa 3 - Base ASDA': 'Entrada > corredor com B5 elite > antecâmara de recuperação > arena com B6 Mark > sequência narrativa final. O sacrifício de Brian e a revelação da profecia integram o desfecho; não são derrota do jogador. Total: dois encontros.',
}

EXTRA = [
 ('Direção visual e acervo recebido', [
 ('Identidade e referências', 'Fantasia urbana juvenil ilustrada em 2D, com influência visual de animação japonesa perceptível nas artes recebidas, contornos definidos e sombreamento simplificado nos personagens. O livro permanece como referência narrativa. Cenários recebidos são mais detalhados e usam perspectiva oblíqua; tratá-los como referência de atmosfera e composição, adaptando a câmera para a exploração planejada.'),
 ('Paleta funcional', dict(VISUAL[0][1])['Paleta funcional']),
 ('Design de Magnus', 'A referência recebida apresenta cabelo escuro curto e bagunçado, olhos castanhos, jaqueta azul-petróleo com faixas laranja e ciano junto à abertura, camiseta clara, calça cinza e tênis claros. Manter esse conjunto nas vistas, expressões e animações. O model sheet e os detalhes estão em Art/ModelSheets/Magnus; o concept está em Art/Concepts/Characters/Magnus.'),
 ('Material disponível', 'O inventário registra 29 PNGs: 8 concepts/retratos de personagens, incluindo a prancha de guardas; 3 model sheets de Magnus; 3 concepts de ambientes; 3 fundos de batalha; 1 prancha de objetos; 1 prancha conjunta de cenários/objetos; 4 estudos de caminhada; 2 estudos de efeitos e 4 mapas/sketches. Os originais foram preservados e as cópias verificadas por SHA-256.'),
 ('Nomenclatura dos ambientes', 'Acampamento corresponde à fase 1; Posto DASA, à empresa de segurança da fase 2; Base ASDA, à instalação final da fase 3. Os originais asda e posto-asda-lateral.png foram classificados como Posto DASA pelo conteúdo. O inventário preserva os nomes de origem para rastreabilidade.'),
 ('Organização e consulta', 'Art/README.md contém links de todas as imagens. Art/inventario.json registra origem, resolução, tamanho e hash. Concepts ficam em Art/Concepts; pranchas do protagonista em Art/ModelSheets; fundos em Art/BattleBackgrounds; movimento e efeitos em Art/AnimationStudies; mapas em LevelDesign. Os anexos deste PDF mostram uma seleção do acervo; o índice contém o conjunto completo.')]),
 ('Sprites, animações e integração', [
 ('Especificações previstas', dict(VISUAL[6][1])['Padrão técnico proposto']),
 ('Magnus: exploração', 'Recebidas quatro pranchas de caminhada, uma por direção, cada uma com quatro poses. São estudos com fundo visível, ainda sem recorte em células e sem animação validada. Meta: 16 quadros de caminhada e 4 poses paradas, cerca de 8 quadros por segundo. Revisar poses repetidas, troca de apoio dos pés, escala e continuidade antes de exportar.'),
 ('Magnus: combate', dict(VISUAL[6][1])['Magnus - combate | 20 quadros']),
 ('NPCs e inimigos', dict(VISUAL[6][1])['NPCs e inimigos']),
 ('Efeitos, objetos e interface', 'Recebidas duas pranchas com seis estágios visuais cada: fogo e água. Recortar e alinhar, preservar arcos de movimento e remover fundo antes de testar. Impacto básico, defesa, sprites estáticos de NPCs, animações de combate, tiles e ícones de interface continuam pendentes. A prancha de objetos apresenta pergaminho, terminal, identificação e painel; não há objetos individuais preparados.'),
 ('Preparação para Unity', 'Produzir cópias derivadas sem modificar as referências. Separar quadros, uniformizar escala, conferir transparência real, definir pivô nos pés e verificar o ciclo. Depois, importar os arquivos finais no projeto Unity e versionar os .meta. Os tamanhos 64 x 64 e 256 x 256 são metas propostas de exportação, não as dimensões das imagens recebidas.'),
 ('Critérios de aprovação técnica', 'Verificar legibilidade na escala de jogo, orientação da câmera, consistência de roupa e silhueta, área livre para menus nos fundos e ausência de deslocamentos bruscos nos ciclos. As imagens ainda não foram validadas em gameplay; apenas leitura, organização e integridade dos arquivos foram verificadas.')]),
 ('Fases e progressão dos desafios', [
 ('Fase 1: aprender | 32 x 24 tiles propostos', 'Dois encontros, B1 treino e B2 guarda; P1 revela a traição. Tempo estimado de 6 a 8 minutos. Ensinar movimento, interação, ataque e defesa; planejar evolução do nível 1 para o 2 ao concluir a área. Usar o sketch como topologia física: entrada ao sul, pátio central, templo ao norte, lago a oeste e trilha a leste. Após P1, retornar ao pátio e liberar a trilha.'),
 ('Fase 2: investigar | 36 x 24 tiles propostos', 'Dois guardas, B3 e B4; P2 identifica a organização e P3 localiza sua base. Tempo estimado de 7 a 10 minutos. Apresentar objetivos e exigir alternância entre dano e proteção. A saída depende das pistas e vitórias, sem impedir retorno ao arquivo ou terminal. Planejar nível 3 ao final, sem repetição de encontros para ganhar XP.'),
 ('Fase 3: combinar | 32 x 28 tiles propostos', 'B5 elite exige uso de defesa; B6 Mark alterna ataque simples e preparação visível de ataque forte, permitindo reação no turno seguinte. Recuperar recursos na antecâmara. Tempo estimado de 7 a 12 minutos; concluir com a cena de Brian e a queda da ASDA. Regras e duração dependem de playtest.'),
 ('Leitura dos mapas', 'Círculos representam encontros; losangos representam pistas nos sketches. Os diagramas não estabelecem colisões, medidas exatas ou câmera final. O diagrama de progressão da fase 1 define sequência de objetivos; para navegação, vale a ligação pátio-trilha descrita acima. Os sketches das fases 2 e 3 ainda precisam de rótulos equivalentes para apresentação.'),
 ('Coerência do escopo', 'Mantidos um personagem controlável, três mapas, seis encontros, três níveis e final único. O total estimado continua próximo de 20 a 30 minutos. Personagens aliados atuam em diálogos e eventos, sem sistema de grupo controlável. Ajustar XP e dificuldade com base nos encontros obrigatórios.'),
 ('Próximas validações', 'Construir uma passagem jogável por fase e testar ordem de objetivos, bloqueios, combate, recompensas únicas e salvamento. Verificar resolução e controles no Android. Não há resultados de playtest ou evidência de integração dos assets nesta revisão.')]),
]

GALLERIES = [
 ('Referências de Magnus', [
  ('Art/ModelSheets/Magnus/magnus-vistas-construcao.png','Vistas de construção recebidas; referência de continuidade.'),
  ('Art/ModelSheets/Magnus/magnus-expressoes.png','Prancha de expressões recebida; conferir diferenciação das emoções.')]),
 ('Referências de cenário e batalha', [
  ('Art/Concepts/Environments/Acampamento/acampamento-exploracao.png','Acampamento: concept com visão oblíqua, referência para exploração.'),
  ('Art/BattleBackgrounds/BaseASDA/base-asda-batalha.png','Base ASDA: fundo lateral recebido para composição de batalha.')]),
 ('Mapas recebidos: fases 1 e 2', [
  ('LevelDesign/Fase01-Acampamento/fase01-sketch.png','Fase 1: templo ao norte, trilha ligada ao pátio a leste.'),
  ('LevelDesign/Fase02-PostoDASA/fase02-sketch.png','Fase 2: recepção, corredor, arquivo e sala de segurança.')]),
 ('Mapa e estudo de efeito', [
  ('LevelDesign/Fase03-BaseASDA/fase03-sketch.png','Fase 3: corredor, antecâmara e arena final.'),
  ('Art/AnimationStudies/Effects/agua-sequencia.png','Água: seis estágios visuais recebidos, ainda com fundo.')]),
]

def main():
    inventory=json.loads((ROOT/'Art/inventario.json').read_text(encoding='utf-8'))
    assert len(inventory)==29
    for r in inventory: assert (ROOT/r['arquivo']).is_file(),r['arquivo']
    pages=[(t.replace('v0.1',VERSION),[(h,REPLACEMENTS.get(h,b)) for h,b in sections]) for t,sections in INITIAL]+EXTRA
    md=['# Between fire and water | GDD '+VERSION,'','Revisão atual: concepção, direção visual e acervo recebido. 10/09/2026.','']
    styles={
      'title':ParagraphStyle('title',fontName='Helvetica-Bold',fontSize=23,leading=28,textColor=HexColor('#152C3D'),spaceAfter=15),
      'head':ParagraphStyle('head',fontName='Helvetica-Bold',fontSize=11,leading=14,textColor=HexColor('#A64B24'),spaceBefore=10,spaceAfter=4),
      'body':ParagraphStyle('body',fontName='Helvetica',fontSize=10,leading=14,textColor=HexColor('#243746'),spaceAfter=4),
      'caption':ParagraphStyle('caption',fontName='Helvetica',fontSize=9,leading=12,textColor=HexColor('#526878'),spaceAfter=12)
    }
    flow=[]
    for i,(title,sections) in enumerate(pages):
        if i: flow.append(PageBreak())
        flow.append(Paragraph(escape(title),styles['title']))
        md += ['## '+title,'']
        for heading,body in sections:
            flow += [Paragraph(escape(heading),styles['head']),Paragraph(escape(body),styles['body'])]
            md += ['### '+heading,'',body,'']
    for title,items in GALLERIES:
        flow += [PageBreak(),Paragraph(escape(title),styles['title'])]
        md += ['## '+title,'']
        for relative,caption in items:
            with PILImage.open(ROOT/relative) as im: w,h=im.size
            factor=min(510/w,270/h)
            flow += [Image(str(ROOT/relative),width=w*factor,height=h*factor),Spacer(1,7),Paragraph(escape(caption),styles['caption'])]
            md += [f'![{caption}](../{relative})','',caption,'']
    md += ['## Índice completo de imagens','','| Arquivo | Dimensões |','| --- | --- |']
    for r in inventory:
        md += [f"| [{r['arquivo']}](../{r['arquivo']}) | {r['largura']} x {r['altura']} |"]
    md += ['','[Inventário técnico](../Art/inventario.json) | [Índice visual](../Art/README.md)','']
    (ROOT/f'docs/GDD-{VERSION}.md').write_text('\n'.join(md),encoding='utf-8')
    def frame(c,d):
        w,h=d.pagesize
        c.setFillColor(HexColor('#152C3D')); c.rect(0,h-46,w,46,fill=1,stroke=0)
        c.setFillColor(HexColor('#FFFFFF')); c.setFont('Helvetica-Bold',10); c.drawString(42,h-28,'BETWEEN FIRE AND WATER / GDD ATUALIZADO')
        c.setFillColor(HexColor('#F47B35')); c.rect(42,h-55,70,3,fill=1,stroke=0)
        c.setFillColor(HexColor('#32BBD3')); c.rect(112,h-55,70,3,fill=1,stroke=0)
        c.setFillColor(HexColor('#526878')); c.setFont('Helvetica',8); c.drawString(42,25,f'{VERSION} | Acervo visual recebido | 10/09/2026'); c.drawRightString(w-42,25,str(d.page))
    path=ROOT/f'output/pdf/Between-fire-and-water-GDD-{VERSION}.pdf'
    SimpleDocTemplate(str(path),pagesize=(595.28,841.89),leftMargin=42,rightMargin=42,topMargin=75,bottomMargin=47,title=f'Between fire and water - GDD {VERSION}',author='Equipe Between fire and water').build(flow,onFirstPage=frame,onLaterPages=frame)
    print(path)

if __name__=='__main__': main()
