using System.Drawing;

class KekImageFormat {
    public int Width { get; private set; }
    public int Hight { get; private set; }

    public Color[,] Palette { get; init; }
    public ColorIndex[,] Pixels { get; init; }

    //private int _bitPerPixel;
    //private int _paletteLenght;

    private const int HEADER_LENGHT = 7;
    private const int HEADER_WIDTH_LENGHT = 2;
    private const int HEADER_HIGHT_LENGHT = 2;
    private const int HEADER_PIXEL_LENGHT = 1;
    private const int HEADER_PALETTE_LENGHT = 2;

    private const int PALETTE_SIZE = 4;
    private const int PALETTE_COLOR_LENGHT = 4;
    private const int PALETTE_COLORS_NUMBER = 16;
    private const int PALETTE_LENGHT = PALETTE_COLOR_LENGHT * PALETTE_COLORS_NUMBER;

    private const int BITS_PER_PIXEL = 4;
    private const int PIXELS_IN_BYTE = 8 / BITS_PER_PIXEL;

    public KekImageFormat(Span<byte> data) {
        Span<byte> headerData = data.Slice(0, HEADER_LENGHT);
        _ParseHeader(headerData);

        Palette = new Color[PALETTE_SIZE, PALETTE_SIZE];
        Span<byte> paletteData = data.Slice(HEADER_LENGHT, PALETTE_LENGHT);
        _ParsePalette(paletteData);

        Pixels = new ColorIndex[Width, Hight];
        const int PIXELS_DATA_POSITION = PALETTE_LENGHT + HEADER_LENGHT;
        Span<byte> pixelsData = data.Slice(PIXELS_DATA_POSITION, Width * Hight / PIXELS_IN_BYTE);
        _ParsePixels(pixelsData);

        //ColorIndex[,] test = this.СuttingOutAFragment(1,2,1,2);
    }

    public KekImageFormat(int width, int hight, Color[,] palette, ColorIndex[,] pixels) {
        Width = width;
        Hight = hight;
        //Palette = new Color[width, hight];
        //palette.CopyTo(Palette, 0);

        Palette = palette;

        Pixels = pixels;
    }

    private void _ParseHeader(Span<byte> headerData) {
        Span<byte> widthBytes = headerData.Slice(0, HEADER_WIDTH_LENGHT);
        Width = BitConverter.ToUInt16(widthBytes);

        Span<byte> hightBytes = headerData.Slice(HEADER_WIDTH_LENGHT, HEADER_HIGHT_LENGHT);
        Hight = BitConverter.ToUInt16(hightBytes);

        //const int PIXEL_LENGHT_POSITION = HEADER_HIGHT_LENGHT + HEADER_WIDTH_LENGHT;
        //Span<byte> bitPerPixelData = headerData.Slice(PIXEL_LENGHT_POSITION, HEADER_PIXEL_LENGHT);
        //_bitPerPixel = bitPerPixelData[0];

        //const int PALETTE_LENGHT_POSITION = PIXEL_LENGHT_POSITION + HEADER_PIXEL_LENGHT;
        //Span<byte> paletteLenghtData = headerData.Slice(PALETTE_LENGHT_POSITION, HEADER_PALETTE_LENGHT);
        //_paletteLenght = BitConverter.ToUInt16(paletteLenghtData);
    }

    private void _ParsePalette(Span<byte> paletteData) {
        int index = 0;
        for (int i = 0; i < PALETTE_COLORS_NUMBER; i++) {
            if (i % PALETTE_SIZE == 0 && i != 0)
                index++;
            Span<byte> colorData = paletteData.Slice(i * PALETTE_COLOR_LENGHT, PALETTE_COLOR_LENGHT);
            colorData[^1] = 0xff;
            int argb = BitConverter.ToInt32(colorData);
            Color color = Color.FromArgb(argb);
            Palette[i % PALETTE_SIZE, index] = color;
        }
    }

    private void _ParsePixels(Span<byte> pixelsData) {
        const int PIXEL_MASK = 0b1111;
        const int Y_INDEX_MASK = 0b11;
        const int X_INDEX_SHIFT = BITS_PER_PIXEL / 2;

        int i = 0;
        int j = 0;
        foreach (var pixelData in pixelsData) {
            int pixel = pixelData & PIXEL_MASK;

            int y = pixel & Y_INDEX_MASK;
            int x = pixel >> X_INDEX_SHIFT;

            Pixels[i, j++] = new ColorIndex(x, y);

            pixel = pixelData >> BITS_PER_PIXEL;

            y = pixel & Y_INDEX_MASK;
            x = pixel >> X_INDEX_SHIFT;

            Pixels[i, j++] = new ColorIndex(x, y);

            if (j == Width) {
                i++;
                j = 0;
            }
        }
    }
    public readonly struct ColorIndex {
        public ColorIndex(int x, int y) {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }
    }

    public KekImageFormat СuttingOutAFragment (int xStart, int xEnd, int yStart, int yEnd) {
        //проверка 
        ColorIndex[,] newMaterix = new ColorIndex[(yEnd-yStart) + 1, (xEnd-xStart) + 1];
        int indX = 0;
        int indY = 0;
        for (int x=yStart; x <= yEnd; x++){
            for (int y = xStart; y <= xEnd; y++) {
                newMaterix[indX, indY] = Pixels[x, y];
                indY++;
            }
            indY = 0;
            indX++;
        }

        return new KekImageFormat(xEnd-xStart + 1, yEnd-yStart + 1, this.Palette, newMaterix);
    }













}
