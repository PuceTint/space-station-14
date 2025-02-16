using Robust.Shared.Audio;
using Robust.Shared.Random;

namespace Content.Shared.Audio;

public static class AudioFun
{
    private static readonly RobustRandom Random = new();

    private const float MinPitchScale = 0.2f;
    private const float MaxPitchScale = 2.0f;

    public static AudioParams FunAudioParams(AudioParams? audioParams = null)
    {
        audioParams ??= AudioParams.Default;

        return audioParams.Value.WithPitchScale(Random.NextFloat() * (MaxPitchScale - MinPitchScale) + MinPitchScale)
            .WithVariation(0);
    }
}
