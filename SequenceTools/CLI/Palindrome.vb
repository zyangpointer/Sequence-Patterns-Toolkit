﻿Imports LANS.SystemsBiology.Assembly.NCBI
Imports LANS.SystemsBiology.Assembly.NCBI.GenBank.TabularFormat.ComponentModels
Imports LANS.SystemsBiology.ComponentModel.Loci
Imports LANS.SystemsBiology.SequenceModel.FASTA
Imports LANS.SystemsBiology.ContextModel
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DocumentFormat.Csv
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Language.UnixBash
Imports LANS.SystemsBiology.AnalysisTools.SequenceTools.SequencePatterns.Topologically
Imports LANS.SystemsBiology.AnalysisTools.SequenceTools.SequencePatterns
Imports Microsoft.VisualBasic.Language

Partial Module Utilities

    <ExportAPI("/Loci.describ",
               Info:="Testing",
               Usage:="/Loci.describ /ptt <genome-context.ptt> [/test <loci:randomize> /complement /unstrand]")>
    Public Function LociDescript(args As CommandLine.CommandLine) As Integer
        Dim PTT = GenBank.TabularFormat.PTT.Load(args("/ptt"))
        Dim test As Integer = args.GetValue("/test", PTT.Size * Rnd())
        Dim complement As Boolean = args.GetBoolean("/complement")
        Dim loci As New NucleotideLocation(test, test + 30, complement)
        Dim unstrand As Boolean = args.GetBoolean("/unstrand")
        Dim lst = PTT.GetRelatedGenes(loci, unstrand)

        For Each g As Relationship(Of GeneBrief) In lst
            Call g.__DEBUG_ECHO
        Next

        Return 0
    End Function

    <ExportAPI("--Palindrome.From.NT",
               Info:="This function is just for debugger test, /nt parameter is the nucleotide sequence data as ATGCCCC",
               Usage:="--Palindrome.From.NT /nt <nt-sequence> /out <out.csv> [/min <3> /max <20>]")>
    Public Function SearchPalindromeNT(args As CommandLine.CommandLine) As Integer
        Dim NT As New FastaToken With {
            .SequenceData = args("/nt"),
            .Attributes = {"auto"}
        }
        Dim Out As String = args("/out")
        Dim Min As Integer = args.GetValue("/min", 3)
        Dim Max As Integer = args.GetValue("/max", 20)
        Dim Search As New Topologically.PalindromeSearchs(NT, Min, Max)
        Call Search.InvokeSearch()
        Call Search.ResultSet.SaveTo(Out)
        Return 0
    End Function

    <ExportAPI("--Palindrome.From.FASTA",
               Usage:="--Palindrome.From.Fasta /nt <nt-sequence.fasta> [/out <out.csv> /min <3> /max <20>]")>
    <ParameterInfo("/nt", False, Description:="Fasta sequence file, and this file should just contains only one sequence.")>
    Public Function SearchPalindromeFasta(args As CommandLine.CommandLine) As Integer
        Dim nt As FastaToken = FastaToken.Load(args("/nt"))
        Dim Out As String = args.GetValue("/out", args("/nt").TrimFileExt & ".csv")
        Dim Min As Integer = args.GetValue("/min", 3)
        Dim Max As Integer = args.GetValue("/max", 20)
        Dim Search As New Topologically.PalindromeSearchs(nt, Min, Max)
        Call Search.InvokeSearch()
        Call Search.ResultSet.SaveTo(Out)
        Return 0
    End Function

    ''' <summary>
    ''' 同一条链上面的镜像回文
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <ExportAPI("--Mirror.From.NT",
               Usage:="--Mirror.From.NT /nt <nt-sequence> /out <out.csv> [/min <3> /max <20>]",
               Info:="Mirror Palindrome, and this function is for the debugging test")>
    Public Function SearchMirrotNT(args As CommandLine.CommandLine) As Integer
        Dim NT As New FastaToken With {
           .SequenceData = args("/nt"),
           .Attributes = {"auto"}
        }
        Dim Out As String = args("/out")
        Dim Min As Integer = args.GetValue("/min", 3)
        Dim Max As Integer = args.GetValue("/max", 20)
        Dim Search As New Topologically.MirrorSearchs(NT, Min, Max)
        Call Search.InvokeSearch()
        Call Search.ResultSet.SaveTo(Out)
        Return 0
    End Function

    <ExportAPI("/Mirror.Batch",
               Usage:="/Mirror.Batch /nt <nt.fasta> [/out <out.csv> /mp /min <3> /max <20>]")>
    <ParameterInfo("/mp", True,
                   Description:="Calculation in the multiple process mode?")>
    Public Function MirrorBatch(args As CommandLine.CommandLine) As Integer
        Dim NT As New FastaFile(args - "/nt")
        Dim out As String = args.GetValue("/out", args("/nt").TrimFileExt & "-Mirror/")
        Dim Min As Integer = args.GetValue("/min", 3)
        Dim Max As Integer = args.GetValue("/max", 20)

        If args.GetBoolean("/mp") Then
            Dim api As String = GetType(Utilities).API(NameOf(SearchMirrotFasta))
            Dim task As Func(Of String, String) =
                Function(path) $"{api} /nt {path.CliPath} /out {(out & "/" & path.BaseName & ".csv").CliPath} /min {Min} /max {Max}"
            Dim CLI As String() =
                LinqAPI.Exec(Of String) <= From fa As FastaToken
                                           In NT
                                           Let path As String = App.GetAppSysTempFile(".fasta")
                                           Let save As Boolean = fa.Save(path, Encodings.ASCII)
                                           Select task(path)
            Call App.SelfFolks(CLI, LQuerySchedule.CPU_NUMBER)
        Else
            For Each seq As FastaToken In NT
                Dim Search As New Topologically.MirrorSearchs(seq, Min, Max)
                Dim path As String = out & $"/{seq.Title.NormalizePathString.Replace(" ", "_")}.csv"
                Call Search.InvokeSearch()
                Call Search.ResultSet.SaveTo(path)
            Next
        End If

        Return 0
    End Function

    ''' <summary>
    ''' 同一条链上面的镜像回文
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <ExportAPI("--Mirror.From.Fasta",
               Usage:="--Mirror.From.Fasta /nt <nt-sequence.fasta> [/out <out.csv> /min <3> /max <20>]",
               Info:="Mirror Palindrome, search from a fasta file.")>
    <ParameterInfo("/nt", False,
                   Description:="This fasta file should contains only just one sequence.")>
    Public Function SearchMirrotFasta(args As CommandLine.CommandLine) As Integer
        Dim Nt = FastaToken.Load(args("/nt"))
        Dim Out As String = args.GetValue("/out", args("/nt").TrimFileExt & ".csv")
        Dim Min As Integer = args.GetValue("/min", 3)
        Dim Max As Integer = args.GetValue("/max", 20)
        Dim Search As New Topologically.MirrorSearchs(Nt, Min, Max)
        Call Search.InvokeSearch()
        Return Search.ResultSet.SaveTo(Out).CLICode
    End Function

    ''' <summary>
    ''' 主要是搜索可能的酶切位点
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <ExportAPI("--Palindrome.batch.Task",
               Usage:="--Palindrome.batch.Task /in <in.fasta> /out <outDir> [/min <3> /max <20> /num_threads <-1>]")>
    Public Function BatchSearchPalindrome(args As CommandLine.CommandLine) As Integer
        Dim input As String = args("/in")
        Dim outDIR As String = args("/out")
        Dim min As Integer = args.GetValue("/min", 3)
        Dim max As Integer = args.GetValue("/max", 20)
        Dim Fasta As FastaFile = FastaFile.Read(input)
        Dim numThreads As Integer = args.GetValue("/num_threads", -1)

        Call BatchTask(Fasta,
                       getCLI:=Function(fa) __palindromeTask(fa, outDIR, min, max),
                       getExe:=Function() App.ExecutablePath,
                       numThreads:=numThreads)
        Return 0
    End Function

    ''' <summary>
    ''' 搜索可能的酶切位点
    ''' </summary>
    ''' <param name="fasta"></param>
    ''' <param name="EXPORT"></param>
    ''' <param name="min"></param>
    ''' <param name="max"></param>
    ''' <returns></returns>
    Private Function __palindromeTask(fasta As FastaToken, EXPORT As String, min As Integer, max As Integer) As String
        Dim csv As String = $"{EXPORT}/{fasta.Title.NormalizePathString(True)}.csv"
        Dim Temp As String = App.GetAppSysTempFile(".fasta")
        Dim CLI As String = $"--Palindrome.From.Fasta /nt {Temp.CliPath} /out {csv.CliPath} /min {min} /max {max}"
        Call fasta.SaveTo(Temp)

        Return CLI
    End Function

    ''' <summary>
    ''' 主要是搜索可能的RNA发卡结构的形成位点
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <ExportAPI("--ImperfectsPalindrome.batch.Task",
               Usage:="--ImperfectsPalindrome.batch.Task /in <in.fasta> /out <outDir> [/min <3> /max <20> /cutoff <0.6> /max-dist <1000 (bp)> /num_threads <-1>]")>
    Public Function BatchSearchImperfectsPalindrome(args As CommandLine.CommandLine) As Integer
        Dim input As String = args("/in")
        Dim out As String = args("/out")
        Dim min As Integer = args.GetValue("/min", 3)
        Dim max As Integer = args.GetValue("/max", 20)
        Dim cutoff As Double = args.GetValue("/cutoff", 0.6)
        Dim maxDist As Integer = args.GetValue("/max-dist", 1000)
        Dim Fasta = FastaFile.Read(input)
        Dim numThreads As Integer = args.GetValue("/num_threads", -1)

        Call BatchTask(Fasta,
                       getCLI:=Function(fa) __imperfectsPalindromeTask(fa, out, min, max, cutoff, maxDist),
                       getExe:=Function() App.ExecutablePath,
                       numThreads:=numThreads)
        Return 0
    End Function

    ''' <summary>
    ''' 搜索可能的RNA发卡结构
    ''' </summary>
    ''' <param name="fasta"></param>
    ''' <param name="EXPORT"></param>
    ''' <param name="min"></param>
    ''' <param name="max"></param>
    ''' <param name="cutoff"></param>
    ''' <param name="maxDist"></param>
    ''' <returns></returns>
    Private Function __imperfectsPalindromeTask(fasta As FastaToken,
                                                EXPORT As String,
                                                min As Integer,
                                                max As Integer,
                                                cutoff As Double,
                                                maxDist As Integer) As String
        Dim csv As String = $"{EXPORT}/{fasta.Title.NormalizePathString(True)}.csv"
        Dim Temp As String = App.GetAppSysTempFile(".fasta")
        Dim CLI As String = $"--Palindrome.Imperfects /in {Temp.CliPath} /out {csv.CliPath} /min {min} /max {max} /cutoff {cutoff} /max-dist {maxDist}"
        Call fasta.SaveTo(Temp)

        Return CLI
    End Function

    <ExportAPI("--Palindrome.Imperfects",
               Usage:="--Palindrome.Imperfects /in <in.fasta> [/out <out.csv> /min <3> /max <20> /cutoff <0.6> /max-dist <1000 (bp)> /partitions <-1>]")>
    Public Function ImperfectPalindrome(args As CommandLine.CommandLine) As Integer
        Dim input As String = args("/in")
        Dim out As String = args.GetValue("/out", input.TrimFileExt & ".csv")
        Dim min As Integer = args.GetValue("/min", 3)
        Dim max As Integer = args.GetValue("/max", 20)
        Dim inFasta As FastaToken
        Dim cutoff As Double = args.GetValue("/cutoff", 0.6)
        Dim maxDist As Integer = args.GetValue("/max-dist", 1000)
        Dim partitions As Integer = args.GetValue("/partitions", -1)

        If input.FileExists Then
            inFasta = FastaToken.Load(input)
        Else
            inFasta = New SequenceModel.FASTA.FastaToken With {
                .SequenceData = input,
                .Attributes = {"auto-generated"}
            }
        End If

        Dim search As New Topologically.Imperfect(inFasta, min, max, cutoff, maxDist, partitions)
        Call search.InvokeSearch()
        Return search.ResultSet.SaveTo(out)
    End Function

    ''' <summary>
    ''' /num_threads &lt;-1>: -1表示使用系统自动分配的参数值
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <ExportAPI("--Hairpinks.batch.task",
               Usage:="--Hairpinks.batch.task /in <in.fasta> [/out <outDIR> /min <6> /max <7> /cutoff <0.6> /max-dist <35 (bp)> /num_threads <-1> /interval <200>]")>
    Public Function HairpinksBatch(args As CommandLine.CommandLine) As Integer
        Dim input As String = args("/in")
        Dim out As String = args.GetValue("/out", App.CurrentDirectory & "/Hairpinks/")
        Dim min As Integer = args.GetValue("/min", 6)
        Dim max As Integer = args.GetValue("/max", 7)
        Dim cutoff As Double = args.GetValue("/cutoff", 0.6)
        Dim maxDist As Integer = args.GetValue("/max-dist", 35)
        Dim inFasta As FastaFile = FastaFile.LoadNucleotideData(input)
        Dim numThreads As Integer = args.GetValue("/num_threads", -1)
        Dim interval As Integer = args.GetValue("/interval", 200)

        Call BatchTask(inFasta,
                       getCLI:=Function(fa) __hairpinksCLI(fa, out, min, max, cutoff, maxDist),
                       getExe:=Function() App.ExecutablePath,
                       numThreads:=numThreads,
                       TimeInterval:=interval)
        Return 0
    End Function

    Private Function __hairpinksCLI(fasta As FastaToken,
                                    EXPORT As String,
                                    min As Integer,
                                    max As Integer,
                                    cutoff As Double,
                                    maxDist As Integer) As String
        Dim csv As String = $"{EXPORT}/{fasta.Title.NormalizePathString(True)}.csv"
        Dim Temp As String = App.GetAppSysTempFile(".fasta")
        Dim CLI As String = $"--Hairpinks /in {Temp.CliPath} /out {csv.CliPath} /min {min} /max {max} /cutoff {cutoff} /max-dist {maxDist}"
        Call fasta.SaveTo(Temp)

        Return CLI
    End Function

    <ExportAPI("--PerfectPalindrome.Filtering", Usage:="--PerfectPalindrome.Filtering /in <inDIR> [/min <8> /out <outDIR>]")>
    Public Function FilterPerfectPalindrome(args As CommandLine.CommandLine) As Integer
        Dim LQuery = (From file As String
                      In FileIO.FileSystem.GetFiles(args("/in"), FileIO.SearchOption.SearchTopLevelOnly, "*.csv")
                      Select file,
                          hrp = file.LoadCsv(Of Topologically.ImperfectPalindrome)).ToArray
        Dim Cut As Integer = args.GetValue("/min", 8)
        Dim Filter = (From x In LQuery
                      Select x.file,
                          perfects = (From loci As Topologically.ImperfectPalindrome
                                      In x.hrp.AsParallel
                                      Where loci.Palindrome.Count("-"c) <> loci.Palindrome.Length AndAlso
                                          loci.Site.Count("-"c) <> loci.Site.Length AndAlso
                                          loci.MaxMatch >= Cut
                                      Select loci).ToArray).ToArray
        Dim out As String = args.GetValue("/out", App.CurrentDirectory & "/Perfects/")

        For Each file In Filter
            Dim name As String = IO.Path.GetFileNameWithoutExtension(file.file)
            Dim path As String = $"{out}/{name}.csv"
            Call file.perfects.SaveTo(path)
        Next

        Return True
    End Function

    <ExportAPI("--Hairpinks", Usage:="--Hairpinks /in <in.fasta> [/out <out.csv> /min <6> /max <7> /cutoff 3 /max-dist <35 (bp)>]")>
    Public Function Hairpinks(args As CommandLine.CommandLine) As Integer
        Dim input As String = args("/in")
        Dim out As String = args.GetValue("/out", input.TrimFileExt & ".hairpink.csv")
        Dim min As Integer = args.GetValue("/min", 6)
        Dim max As Integer = args.GetValue("/max", 7)
        Dim cutoff As Integer = args.GetValue("/cutoff", 3)
        Dim maxDist As Integer = args.GetValue("/max-dist", 35)
        Dim inFasta As FastaToken = FastaToken.Load(input)

        Dim search As New TextIndexing(inFasta.SequenceData, min, max)
        Dim resultSet = (From segment As TextSegment
                         In search.PreCache
                         Let result = Found(inFasta, segment, cutoff, search, maxDist, max)
                         Where Not result Is Nothing
                         Select result).ToArray

        Return resultSet.SaveTo(out)
    End Function

    Private Function Found(inFasta As FastaToken,
                           segment As TextSegment,
                           cutoff As Integer,
                           search As TextIndexing,
                           maxDist As Integer,
                           max As Integer) As ImperfectPalindrome
        Dim palin As String = PalindromeLoci.GetPalindrome(segment.Segment)  ' 当前片段所计算出来的完全匹配的回文位点
        Dim start As Integer = segment.Index + segment.Array.Length + maxDist * 0.95
        Dim parPiece As String = Mid(inFasta.SequenceData, start, max + 5)  ' 实际的位点
        Dim dist = LevenshteinDistance.ComputeDistance(palin, parPiece)

        If dist Is Nothing Then Return Nothing

        Dim maxMatch As Integer = search.IsMatch(dist.DistEdits, cutoff)

        If maxMatch <= 0 Then Return Nothing

        Dim result As New ImperfectPalindrome With {
            .Site = segment.Segment,
            .Left = segment.Index,
            .Palindrome = parPiece,
            .Paloci = start,
            .Distance = dist.Distance,
            .Evolr = dist.DistEdits,
            .Matches = dist.Matches,
            .Score = dist.Score,
            .MaxMatch = maxMatch
        }
        Return result
    End Function

    <ExportAPI("--ToVector", Usage:="--ToVector /in <in.DIR> /min <4> /max <8> /out <out.txt> /size <genome.size>")>
    Public Function ToVector(args As CommandLine.CommandLine) As Integer
        Dim inDIR As String = args("/in")
        Dim min = args.GetInt32("/min")
        Dim max = args.GetInt32("/max")
        Dim out As String = args("/out")
        Dim size As Integer = args.GetInt32("/size")
        Dim vector = Topologically.Palindrome.ImperfectPalindromeVector(inDIR, size, min, max)
        Return vector.ToArray(Function(n) CStr(n)).FlushAllLines(out).CLICode
    End Function

    <ExportAPI("/Mirror.Vector", Usage:="/Mirror.Vector /in <inDIR> /size <genome.size> [/out out.txt]")>
    Public Function MirrorsVector(args As CommandLine.CommandLine) As Integer
        Dim inDIR As String = args("/in")
        Dim out As String = args.GetValue("/out", inDIR.TrimDIR & ".Mirror.Vector.txt")
        Dim files As IEnumerable(Of String) = ls - l - r - wildcards("*.csv") <= inDIR
        Dim size As Integer = args.GetInt32("/size")
        Dim Loads = (From path As String
                     In files
                     Select path.BaseName,
                         data = path.LoadCsv(Of PalindromeLoci)) _
                         .ToDictionary(Function(x) x.BaseName,
                                       Function(x) x.data)
        Dim result As Double() = Topologically.Palindrome.Density(Loads.Values, size)
        Return result.FlushAllLines(out)
    End Function
End Module