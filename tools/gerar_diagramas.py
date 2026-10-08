"""Diagramas do GDD: fontes vetoriais, PNGs e HTML para importação no Canva."""
from pathlib import Path
from html import escape
import math
import subprocess
from reportlab.graphics.shapes import Drawing, Rect, String, Polygon, PolyLine
from reportlab.graphics import renderPDF, renderSVG
from reportlab.lib.colors import HexColor

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'imagens/07-diagramas'
NAVY, ORANGE, CYAN, GRAY = '#152C3D', '#E17A3C', '#259EBC', '#526878'
CREAM, PALE = '#F4F1E8', '#EAF5F8'


class Diagram:
    def __init__(self, name, title, subtitle, height):
        self.name, self.height = name, height
        self.d = Drawing(1200, height)
        self.html = []
        self.rect(0, 0, 1200, height, '#FFFFFF', '#FFFFFF', 0)
        self.rect(0, 0, 1200, 130, NAVY, NAVY, 0)
        self.text(48, 44, 'BETWEEN FIRE AND WATER  /  GDD v0.2', 17, CREAM, 'start')
        self.text(48, 86, title, 34, '#FFFFFF', 'start', True)
        self.text(48, 162, subtitle, 22, GRAY, 'start')
        self.rect(48, 122, 110, 8, ORANGE, ORANGE, 0)
        self.rect(158, 122, 110, 8, CYAN, CYAN, 0)

    def rect(self, x, y, w, h, fill, stroke=NAVY, radius=16):
        self.d.add(Rect(x, self.height-y-h, w, h, rx=radius, ry=radius,
                        fillColor=HexColor(fill), strokeColor=HexColor(stroke), strokeWidth=2))
        self.html.append(f'<div style="position:absolute;left:{x}px;top:{y}px;width:{w}px;height:{h}px;background:{fill};border:2px solid {stroke};border-radius:{radius}px;box-sizing:border-box"></div>')

    def text(self, x, y, text, size=24, color=NAVY, anchor='middle', bold=False):
        self.d.add(String(x, self.height-y, text, fontSize=size,
                          fontName='Helvetica-Bold' if bold else 'Helvetica',
                          fillColor=HexColor(color), textAnchor=anchor))
        transform = 'translateX(-50%)' if anchor == 'middle' else 'none'
        self.html.append(f'<div style="position:absolute;left:{x}px;top:{y-size}px;transform:{transform};font-size:{size}px;font-weight:{700 if bold else 400};color:{color};white-space:nowrap;line-height:1.2">{escape(text)}</div>')

    def box(self, x, y, w, h, lines, color=CYAN, size=24, rounded=False):
        self.rect(x, y, w, h, PALE if color==CYAN else '#FFF1E5', color, h/2 if rounded else 14)
        lines = lines.split('|')
        for i, line in enumerate(lines):
            self.text(x+w/2, y+h/2+(i-(len(lines)-1)/2)*(size+6)+size*.35,
                      line, size, NAVY, bold=i==0)

    def diamond(self, cx, cy, w, h, lines):
        pts=[cx,cy-h/2,cx+w/2,cy,cx,cy+h/2,cx-w/2,cy]
        self.d.add(Polygon([v if i%2==0 else self.height-v for i,v in enumerate(pts)],
                           fillColor=HexColor(CREAM),strokeColor=HexColor(NAVY),strokeWidth=2))
        self.html.append(f'<svg style="position:absolute;left:{cx-w/2}px;top:{cy-h/2}px" width="{w}" height="{h}"><polygon points="{w/2},0 {w},{h/2} {w/2},{h} 0,{h/2}" fill="{CREAM}" stroke="{NAVY}" stroke-width="2"/></svg>')
        ls=lines.split('|')
        for i,line in enumerate(ls):
            self.text(cx,cy+(i-(len(ls)-1)/2)*27+8,line,22,bold=True)

    def arrow(self, pts, color=GRAY):
        coords=[v if i%2==0 else self.height-v for i,v in enumerate(sum(([x,y] for x,y in pts),[]))]
        self.d.add(PolyLine(coords,strokeColor=HexColor(color),strokeWidth=3,fillColor=None))
        x,y=pts[-1]; px,py=pts[-2]; a=math.atan2(y-py,x-px)
        wings=[(x-12*math.cos(a)+6*math.sin(a),y-12*math.sin(a)-6*math.cos(a)),
               (x-12*math.cos(a)-6*math.sin(a),y-12*math.sin(a)+6*math.cos(a))]
        self.d.add(Polygon([x,self.height-y,wings[0][0],self.height-wings[0][1],wings[1][0],self.height-wings[1][1]],fillColor=HexColor(color),strokeColor=HexColor(color)))
        poly=' '.join(f'{a},{b}' for a,b in pts)
        tri=' '.join(f'{a},{b}' for a,b in [(x,y),*wings])
        self.html.append(f'<svg style="position:absolute;left:0;top:0;pointer-events:none" width="1200" height="{self.height}"><polyline points="{poly}" fill="none" stroke="{color}" stroke-width="3"/><polygon points="{tri}" fill="{color}"/></svg>')

    def save(self):
        OUT.mkdir(parents=True,exist_ok=True)
        renderSVG.drawToFile(self.d,str(OUT/f'{self.name}.svg'))
        scratch=ROOT/'tmp/pdfs';scratch.mkdir(parents=True,exist_ok=True)
        pdf=scratch/f'{self.name}.pdf'
        renderPDF.drawToFile(self.d,str(pdf))
        subprocess.run(['pdftoppm','-singlefile','-scale-to','2400','-png',str(pdf),str(OUT/self.name)],check=True,capture_output=True)
        document=f'<!doctype html><html lang="pt-BR"><meta charset="utf-8"><title>{self.name}</title><body style="margin:0;font-family:Arial,Helvetica,sans-serif"><main data-document-role="page" data-label="{self.name}" style="position:relative;width:1200px;height:{self.height}px;overflow:hidden;background:white">'+''.join(self.html)+'</main></body></html>'
        (OUT/f'{self.name}.html').write_text(document,encoding='utf-8')


def core():
    g=Diagram('core-loop','Core loop | ciclo principal','Ações recorrentes que conectam descoberta, desafio e progressão.',900)
    g.box(55,230,310,135,'01  EXPLORAR|Percorrer o mapa|e encontrar interações',size=25)
    g.box(445,230,310,135,'02  INVESTIGAR|Examinar pistas|e identificar o objetivo',size=25)
    g.box(835,230,310,135,'03  COMBATER|Escolher ações em turnos|e vencer o encontro',ORANGE,25)
    g.box(835,555,310,135,'04  RECEBER XP|Ganhar a recompensa|uma vez por encontro',ORANGE,25)
    g.box(445,555,310,135,'05  EVOLUIR|Ao atingir o limiar de XP,|melhorar nível e atributos',ORANGE,24)
    g.box(55,555,310,135,'06  AVANÇAR|Cumprir os objetivos|e liberar a próxima área',size=25)
    for pts in [[(365,298),(445,298)],[(755,298),(835,298)],[(990,365),(990,555)],[(835,623),(755,623)],[(445,623),(365,623)],[(210,555),(210,365)]]:g.arrow(pts)
    g.text(600,435,'DESCOBRIR  •  DECIDIR  •  FORTALECER',23,NAVY,bold=True)
    g.text(600,475,'Magnus aprende e enfrenta novos desafios.',23,GRAY)
    g.text(1030,463,'Vitória',22,ORANGE,'start',True)
    g.text(170,463,'Repetir',22,CYAN,'middle',True)
    g.rect(55,746,1090,106,CREAM,CREAM)
    g.text(80,780,'CONDIÇÕES DO CICLO',19,NAVY,'start',True)
    g.text(80,814,'Derrota: reiniciar o encontro. Encerramento: vencer Mark e concluir o epílogo.',23,NAVY,'start')
    g.save()


def flow():
    g=Diagram('fluxo-do-jogo','Diagrama de fluxo | da entrada ao final','Retângulos = ações  •  Losangos = decisões  •  Setas = transições',1500)
    g.text(300,214,'EXPLORAÇÃO E CAMPANHA',23,CYAN,bold=True)
    g.text(900,214,'COMBATE POR TURNOS',23,ORANGE,bold=True)
    # Left lane; the continuing path occupies the center, returns use left margin.
    g.box(150,245,330,64,'Menu inicial',size=25,rounded=True)
    g.box(115,360,400,80,'Novo jogo / continuar|Iniciar / restaurar progresso',size=23)
    g.arrow([(315,309),(315,360)])
    g.box(115,490,400,92,'Explorar e investigar|Conversar, obter pistas|e atualizar o objetivo',size=23)
    g.arrow([(315,440),(315,490)])
    g.diamond(315,678,330,116,'Inimigo visível|alcançado?')
    g.arrow([(315,582),(315,620)])
    g.diamond(315,895,360,126,'Objetivos da área|concluídos?')
    g.arrow([(315,736),(315,832)]);g.text(340,792,'Não',21,GRAY,'start')
    g.arrow([(135,895),(55,895),(55,536),(115,536)])
    g.text(85,865,'Não',21,GRAY)
    g.box(145,1008,340,68,'Salvar conclusão da área',size=23)
    g.arrow([(315,958),(315,1008)]);g.text(340,990,'Sim',21,GRAY,'start')
    g.diamond(315,1170,330,110,'Mark vencido|na área final?')
    g.arrow([(315,1076),(315,1115)])
    g.box(560,1130,300,80,'Próxima área|Retomar exploração',size=23)
    g.arrow([(480,1170),(560,1170)]);g.text(520,1150,'Não',21,GRAY)
    g.arrow([(710,1130),(710,1090),(620,1090),(620,470),(315,470),(315,490)])
    g.box(100,1290,430,92,'Evento de Brian e epílogo|Campanha concluída / créditos',size=23,rounded=True)
    g.arrow([(315,1225),(315,1290)]);g.text(341,1264,'Sim',21,GRAY,'start')
    # Combat lane. Connectors A and B avoid crossing the campaign's decisions.
    g.arrow([(480,678),(550,678)])
    g.text(521,649,'Sim',21,GRAY)
    g.box(550,651,44,54,'A',ORANGE,22,True)
    g.box(672,252,44,54,'A',ORANGE,22,True)
    g.arrow([(716,279),(750,279)])
    g.box(750,245,350,68,'Iniciar encontro',ORANGE,24)
    g.box(750,365,350,92,'Escolher ação de Magnus|Ataque, fogo, água ou defesa|Executar ação válida',ORANGE,22)
    g.arrow([(925,313),(925,365)])
    g.diamond(925,550,320,110,'Vida do inimigo|chegou a zero?')
    g.arrow([(925,457),(925,495)])
    g.box(750,662,350,68,'Executar ação do inimigo',ORANGE,23)
    g.arrow([(925,605),(925,662)]);g.text(948,642,'Não',21,GRAY,'start')
    g.diamond(925,824,320,110,'Vida de Magnus|chegou a zero?')
    g.arrow([(925,730),(925,769)])
    g.arrow([(1085,824),(1144,824),(1144,410),(1100,410)])
    g.text(1144,862,'Não',21,GRAY)
    g.box(750,944,350,92,'Derrota / tentar novamente|Restaurar estado pré-encontro|Sem perder nível ou duplicar XP',ORANGE,21)
    g.arrow([(925,879),(925,944)]);g.text(949,914,'Sim',21,GRAY,'start')
    g.arrow([(1100,990),(1172,990),(1172,279),(1100,279)])
    # Victory is resolved before the enemy can act.
    g.box(640,535,44,54,'B',ORANGE,22,True)
    g.arrow([(765,550),(705,550),(705,562),(684,562)])
    g.text(723,530,'Sim',21,GRAY)
    g.box(655,1250,44,54,'B',ORANGE,22,True)
    g.arrow([(699,1277),(740,1277)])
    g.box(740,1227,385,100,'Vitória: registrar encontro|Conceder XP uma única vez|Subir de nível se atingir limiar',ORANGE,22)
    g.box(740,1360,385,55,'Retomar verificação dos objetivos',size=21)
    g.arrow([(925,1327),(925,1360)])
    g.box(663,1360,44,55,'C',size=22,rounded=True)
    g.arrow([(740,1388),(707,1388)])
    g.box(536,868,44,54,'C',size=22,rounded=True)
    g.arrow([(536,895),(495,895)])
    g.text(48,1460,'A / B / C conectam trechos do fluxo. Pausa: retomar, ajustar volume ou voltar ao menu.',21,GRAY,'start')
    g.save()


if __name__=='__main__':
    core();flow()
    print(OUT)
