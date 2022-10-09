using SFML;
using SFML.Graphics;
using SFML.Window;

string imagePath = "C:\\Users\\konov\\Desktop\\image - Copy.kek";
var imageData = File.ReadAllBytes(imagePath);

var kek = new KekImageFormat(imageData);
var converter = new KekToTextureConverter();

var texture = converter.Convert(kek);
var sprite = new Sprite(texture);
sprite.Scale *= 20;

var window = new RenderWindow(new VideoMode(200, 200), "Image");
window.Closed += (_, _) => window.Close();
while (window.IsOpen) {
    window.DispatchEvents();

    window.Clear();

    window.Draw(sprite);

    window.Display();
}
