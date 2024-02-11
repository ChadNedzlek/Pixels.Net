using System.Reflection;
using NUnit.Framework;
using VaettirNet.PixelsDice.Net.Animations;

namespace VaettirNet.PixelsDice.Net.Tests;

public class BleManagerTests
{
    [Test]
    public void EnsureAnimationSize()
    {
        foreach (var assembly in Assembly.GetAssembly(typeof(Animation)).GetTypes())
        {
        }
    }
}