using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Page_Navigation_App.Utilities;

public class ModelUtil
{
    public const string scriptname = "test.py";
    public const string py = "python";
    public const string exe = "cmd";
    public const string working_esr = "esr";
    public const string working_resr = "resr";
    public const string working_exe = "resrnoenv";
    
    public const string InputPath1 =  @"esr\input";
    public const string InputPath2 =  @"resr\inputs";
    public const string InputPath3 =  "resrnoenv";
    public const string OutputPath1 = @"esr\results\test.png";
    public const string OutputPath2 = @"resr\results\test_output.png";
    public const string OutputPath3 = @"resrnoenv\output.png";
    
    public static void GetRes(string name, string script, string working)
    {
        var psi = new ProcessStartInfo
        {
            FileName = name,
            Arguments = script,
            UseShellExecute = false,
            WorkingDirectory = working,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            RedirectStandardError = true
        };

        using (var process = new Process())
        {
            process.StartInfo = psi;
            process.OutputDataReceived += (sender, data) => Console.WriteLine(data.Data);
            process.ErrorDataReceived += (sender, data) => Console.WriteLine(data.Data);
            Console.WriteLine($"Starting process: {name} {script} in {working}");
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            
            var exitCode = process.ExitCode;
            Console.WriteLine($"Process exited with code {exitCode}");
        }

    }
    
    
    
}