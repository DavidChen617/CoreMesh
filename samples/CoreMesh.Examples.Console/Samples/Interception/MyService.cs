namespace CoreMesh.Examples.Console.Samples.Interception;

public class MyService: IMyService
{
    public async Task<List<int>> Add(int a, int b)
    {
        await Task.Delay(10);
        System.Console.WriteLine($"{a} + {b}");

        return [a + b];
    }
}
