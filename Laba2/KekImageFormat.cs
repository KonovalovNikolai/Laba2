using System;
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
        Span<byte> pixelsData = data.Slice(PIXELS_DATA_POSITION);
        _ParsePixels(pixelsData);
    }

    public KekImageFormat(int width, int hight, Color[,] palette, ColorIndex[,] pixels) {
        Width = width;
        Hight = hight;

        Palette = palette;

        Pixels = pixels;
    }

    public byte[] ToByteArray() {
        int lenght = Width * Hight;
        lenght = lenght % 2 == 1 ? lenght + 1 : lenght;
        int bytesForPixels = lenght / 2;
        
        int arraySize = HEADER_LENGHT + PALETTE_LENGHT + bytesForPixels;
        byte[] data = new byte[arraySize];

        Span<byte> header = data.AsSpan().Slice(0, HEADER_LENGHT);
        _ConvertHeader(header);

        Span<byte> palette = data.AsSpan().Slice(HEADER_LENGHT, PALETTE_LENGHT);
        _ConvertPalette(palette);

        const int PIXELS_DATA_POSITION = PALETTE_LENGHT + HEADER_LENGHT;
        Span<byte> pixels = data.AsSpan().Slice(PIXELS_DATA_POSITION);
        _ConvertPixels(pixels);

        return data;
    }

    private void _ConvertPixels(Span<byte> pixels) {
        int pixelCount = 0;
        int index = 0;
        int data = 0;
        foreach (var pixel in Pixels) {
            int pixelData = 0;
            pixelData = pixel.Y | (pixel.X << 2);

            if(pixelCount % PIXELS_IN_BYTE == 0) {
                data = pixelData;
            }
            else {
                data |= pixelData << BITS_PER_PIXEL;
                pixels[index++] = (byte)data;
            }

            if (Width * Hight % 2 == 1) {
                pixels[index] = (byte)data;
            }

            pixelCount++;
        }
    }

    private void _ConvertHeader(Span<byte> header) {
        const int HIGHT_POSITION = HEADER_WIDTH_LENGHT;
        const int PIXEL_LENGHT_POSITION = HIGHT_POSITION + HEADER_HIGHT_LENGHT;
        const int PALETTE_LENGHT_POSITION = PIXEL_LENGHT_POSITION + HEADER_PIXEL_LENGHT;

        BitConverter.TryWriteBytes(header, (ushort)Width);
        BitConverter.TryWriteBytes(header.Slice(HIGHT_POSITION), (ushort)Hight);
        BitConverter.TryWriteBytes(header.Slice(PIXEL_LENGHT_POSITION), (byte)BITS_PER_PIXEL);
        BitConverter.TryWriteBytes(header.Slice(PALETTE_LENGHT_POSITION), (ushort)PALETTE_COLORS_NUMBER);
    }

    private void _ConvertPalette(Span<byte> palette) {
        int index = 0;
        foreach (var color in Palette) {
            BitConverter.TryWriteBytes(palette.Slice(index*4), color.ToArgb());
            index++;
        }
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
            int argb = BitConverter.ToInt32(colorData);
            Color color = Color.FromArgb(argb);
            Palette[i % PALETTE_SIZE, index] = color;
        }
    }

    private void _ParsePixels(Span<byte> pixelsData) {
        const int PIXEL_MASK = 0b1111;
        const int Y_INDEX_MASK = 0b11;
        const int X_INDEX_SHIFT = BITS_PER_PIXEL / 2;

        int byteIndex = 0;
        int pixelCounter = 0;
        for (int i = 0; i < Pixels.GetLength(0); i++) {
            for (int j = 0; j < Pixels.GetLength(1); j++) {
                int pixelData;
                if (pixelCounter % 2 == 0) {
                    pixelData = pixelsData[byteIndex] & PIXEL_MASK;
                }
                else {
                    pixelData = pixelsData[byteIndex] >> BITS_PER_PIXEL;
                    byteIndex++;
                }

                int y = pixelData & Y_INDEX_MASK;
                int x = pixelData >> X_INDEX_SHIFT;

                Pixels[i, j] = new ColorIndex(x, y);

                pixelCounter++;
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

    public KekImageFormat СuttingOutAFragment (int yStart, int yEnd, int xStart, int xEnd) { 
        ColorIndex[,] newMaterix = new ColorIndex[(xEnd-xStart) + 1, (yEnd-yStart) + 1];
        int indX = 0;
        int indY = 0;
        for (int x=xStart; x <= xEnd; x++){
            for (int y = yStart; y <= yEnd; y++) {
                newMaterix[indX, indY] = Pixels[x, y];
                indY++;
            }
            indY = 0;
            indX++;
        }

        return new KekImageFormat(yEnd-yStart + 1, xEnd-xStart + 1, this.Palette, newMaterix);
    }













}
