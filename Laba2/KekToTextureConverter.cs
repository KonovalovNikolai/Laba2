using SFML.Graphics;

class KekToTextureConverter {
    public Texture Convert(KekImageFormat image) {
        const int BYTES_PER_COLOR = 4;

        byte[] pixels = new byte[image.Pixels.Length * BYTES_PER_COLOR];
        Span<byte> pixelsData = pixels;

        int index = 0;
        foreach (var pixel in image.Pixels) {
            Span<byte> pixelPosition = pixelsData.Slice(index * BYTES_PER_COLOR);

            int argb = image.Palette[pixel.X, pixel.Y].ToArgb();
            int rgba = argb << 8;
            Span<byte> bytes = BitConverter.GetBytes(rgba);
            bytes.Reverse();
            bytes[^1] = 0xFF;

            bytes.CopyTo(pixelPosition);
            index++;
        }

        var texture = new Texture((uint)image.Width, (uint)image.Hight);
        texture.Update(pixels);
        return texture;
    }
}