using Experimentation.NFA;

if (args.Length != 1)
{
    Console.Error.WriteLine("Program expects one argument - the path to .emp file");
    return -1;
}

if ((new EmpParser(args[0])).parseAndCheckEmptiness())
{
    Console.WriteLine("EMPTY");
}
else
{
    Console.WriteLine("NOT EMPTY");
}
return 0;
