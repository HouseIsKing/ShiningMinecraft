namespace MinecraftLibrary.Engine;

public class PerlinNoise
{
    private readonly int _octave;
    private const int Fuzziness = 16;

    public PerlinNoise(int octave)
    {
        _octave = octave;
    }

    public List<int> Generate(int width, int height)
    {
        var random = new Random();
        var result = new List<int>();
        result.EnsureCapacity(width * height);
        var table = new List<int>();
        table.EnsureCapacity(width * height);
        for (var i = 0; i < width * height; i++)
        {
            table.Add(0);
            result.Add(0);
        }

        var step = width >> _octave;
        for (var y = 0; y < height; y += step)
        for (var x = 0; x < width; x += step)
            table[x + y * width] = (random.NextInt(256) - 128) * Fuzziness;

        for (; step > 1; step /= 2)
        {
            var max = 256 * (step << _octave);
            var halfStep = step >> 1;
            for (var y = 0; y < height; y += step)
            for (var x = 0; x < width; x += step)
            {
                var value = table[x % width + y % height * width];
                var stepValueX = table[(x + step) % width + y % height * width];
                var stepValueY = table[x % width + (y + step) % height * width];
                var stepValueXy = table[(x + step) % width + (y + step) % height * width];
                var mutatedValue = (value + stepValueY + stepValueX + stepValueXy) / 4 + (random.NextInt(2 * max) - max);
                table[x + halfStep + (y + halfStep) * width] = mutatedValue;
            }

            for (var y = 0; y < height; y += step)
            for (var x = 0; x < width; x += step)
            {
                var value = table[x + y * width];
                var stepValueX = table[(x + step) % width + y * width];
                var stepValueY = table[x + (y + step) % height * width];
                var halfStepValueXPos = table[((x + halfStep) & (width - 1)) + ((y + halfStep - step) & (height - 1)) * width];
                var halfStepValueYPos = table[((x + halfStep - step) & (width - 1)) + ((y + halfStep) & (height - 1)) * width];
                var halfStepValue = table[(x + halfStep) % width + (y + halfStep) % height * width];
                var mutatedValueX = (value + stepValueX + halfStepValue + halfStepValueXPos) / 4 + (random.NextInt(2 * max) - max);
                var mutatedValueY = (value + stepValueY + halfStepValue + halfStepValueYPos) / 4 + (random.NextInt(2 * max) - max);
                table[x + halfStep + y * width] = mutatedValueX;
                table[x + (y + halfStep) * width] = mutatedValueY;
            }
        }

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
            result[x + y * width] = table[x % width + y % height * width] / 512 + 128;
        return result;
    }
}