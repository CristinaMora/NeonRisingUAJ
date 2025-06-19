import matplotlib.pyplot as plt
import matplotlib.image as mpimg
from matplotlib.backends.backend_pdf import PdfPages

class PDFWriter:
    def __init__(self, filename='output.pdf'):
        # Al crear la clase, se inicializa un archivo PDF con el nombre dado
        self.filename = filename
        self.pdf = PdfPages(self.filename)
        self.elements = []  # Aqui se guardan textos y graficos para agregarlos luego

        # Tamanyo de la pagina en pulgadas (como hoja A4)
        self.pageWidth = 8.5
        self.pageHeight = 11
        self.margins = {
            'top': 0.5,
            'bottom': 0.5,
            'left': 0.5,
            'right': 0.5
        }

        # Alto estimado para cada linea de texto y grafico
        self.lineHeight = 0.35
        self.graphHeight = 2.5

    def addText(self, text, fontSize=12):
        """Guarda una linea de texto para agregar al PDF"""
        self.elements.append(('text', text, fontSize))

    def addGraph(self, categories, values):
        """Guarda un grafico de barras para agregar al PDF"""
        self.elements.append(('graph', categories, values))
    
    def addImage(self, image_path, image_height, image_width):
        """Guarda una imagen para agregar al PDF con alto y ancho personalizados (en pulgadas)"""
        self.elements.append(('image', image_path, image_height, image_width))

    def _createPage(self, startIndex):
        """Genera una pagina nueva con elementos a partir del indice dado"""
        # Se crea una figura del tamanyo de la pagina
        fig = plt.figure(figsize=(self.pageWidth, self.pageHeight))
        ax = fig.add_axes([0, 0, 1, 1])  # Ejes que cubren toda la pagina
        ax.axis('off')  # No se muestran ejes

        # Posiciones iniciales para colocar contenido
        yCursor = 1 - self.margins['top'] / self.pageHeight
        xCursor = self.margins['left'] / self.pageWidth
        minY = self.margins['bottom'] / self.pageHeight

        idx = startIndex
        while idx < len(self.elements):
            elemType, *data = self.elements[idx]

            if elemType == 'text':
                text, fontSize = data
                # Se calcula el espacio que ocupa esta linea
                lineHeightNorm = (self.lineHeight * fontSize / 12) / self.pageHeight

                if yCursor - lineHeightNorm < minY:
                    break  # No hay espacio, se deja para la siguiente pagina

                ax.text(
                    xCursor, yCursor, text,
                    ha='left', va='top',
                    wrap=True, fontsize=fontSize,
                    transform=ax.transAxes
                )
                yCursor -= lineHeightNorm

            elif elemType == 'graph':
                categories, values = data
                graphHeightNorm = self.graphHeight / self.pageHeight

                if yCursor - graphHeightNorm < minY:
                    break  # No cabe el grafico, se deja para la proxima pagina

                # Se agrega el grafico en un nuevo espacio
                graphAx = fig.add_axes([
                    xCursor,
                    yCursor - graphHeightNorm,
                    1 - self.margins['left'] / self.pageWidth - self.margins['right'] / self.pageWidth,
                    graphHeightNorm
                ])
                graphAx.bar(categories, values, color = 'orange')
                graphAx.set_title('Grafico de Barras')
                graphAx.set_xlabel('Categorias')
                graphAx.set_ylabel('Valores')
                yCursor -= graphHeightNorm

            elif elemType == 'image':
                image_path, image_height, image_width = data
                imageHeightNorm = image_height / self.pageHeight
                imageWidthNorm = image_width / self.pageWidth
                if yCursor - imageHeightNorm < minY:
                    break  # No cabe la imagen, se deja para la proxima pagina
                img = mpimg.imread(image_path)
                imgAx = fig.add_axes([
                    xCursor,
                    yCursor - imageHeightNorm,
                    imageWidthNorm,
                    imageHeightNorm
                ])
                imgAx.imshow(img)
                imgAx.axis('off')
                yCursor -= imageHeightNorm
            idx += 1

        self.pdf.savefig(fig)  # Se guarda la pagina generada en el PDF
        plt.close(fig)
        return idx  # Retorna el indice donde se quedo para continuar luego

    def savePages(self):
        """Crea y guarda todas las paginas necesarias en el PDF."""
        idx = 0
        while idx < len(self.elements):
            idx = self._createPage(idx)
        self.elements.clear()  # Se limpian los elementos ya usados

    def close(self):
        """Guarda el PDF final y cierra el archivo correctamente."""
        self.savePages()
        self.pdf.close()