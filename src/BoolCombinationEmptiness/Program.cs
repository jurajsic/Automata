using Experimentation.NFA;

if (args.Length != 1)
{
    Console.Error.WriteLine("Program expects one argument - the path to .emp file");
    return -1;
}

try {
    if ((new EmpParser(args[0])).parseAndCheckEmptiness())
    {
        Console.WriteLine("result: EMPTY");
    }
    else
    {
        Console.WriteLine("result: NOT EMPTY");
    }
} catch (Exception ex) {
    Console.Error.WriteLine("error: " + ex.Message);
    return -1;
}
return 0;
