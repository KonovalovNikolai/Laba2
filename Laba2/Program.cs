using SFML;
using SFML.Graphics;
using SFML.Window;

string imagePath = "C:\\Users\\konov\\Desktop\\3x3.kek";
var imageData = File.ReadAllBytes(imagePath);

var kek = new KekImageFormat(imageData);
File.WriteAllBytes("C:\\Users\\konov\\Desktop\\test.kek", kek.ToByteArray());

var converter = new KekToTextureConverter();


var test = kek.СuttingOutAFragment(1,2,0,3);
//                                  X    Y


var texture = converter.Convert(test);
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
