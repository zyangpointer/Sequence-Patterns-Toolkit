﻿Imports LANS.SystemsBiology.SequenceModel
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Parallel.Tasks
Imports Microsoft.VisualBasic.Scripting.MetaData

''' <summary>
''' Clustal Omega(多序列比对工具)
''' Clustal Omega - 1.2.0 (AndreaGiacomo)
'''
''' If you Like Clustal - Omega please cite:
''' Sievers F, Wilm A, Dineen D, Gibson TJ, Karplus K, Li W, Lopez R, McWilliam H, Remmert M, Sding J, Thompson JD, Higgins DG.
''' Fast, scalable generation Of high-quality protein multiple sequence alignments Using Clustal Omega.
''' Mol Syst Biol. 2011 Oct 11;7:539. doi: 10.1038/msb.2011.75. PMID: 21988835.
''' If you don't like Clustal-Omega, please let us know why (and cite us anyway).
'''
''' Check http : //www.clustal.org for more information And updates.
''' </summary>
''' <remarks></remarks>
''' 
<[PackageNamespace]("Clustal",
                    Cites:="Sievers, F., et al. (2011). ""Fast, scalable generation Of high-quality protein multiple sequence alignments Using Clustal Omega."" Mol Syst Biol 7: 539.
<p>Multiple sequence alignments are fundamental to many sequence analysis methods. Most alignments are computed using the progressive alignment heuristic. These methods are starting to become a bottleneck in some analysis pipelines when faced with data sets of the size of many thousands of sequences. Some methods allow computation of larger data sets while sacrificing quality, and others produce high-quality alignments, but scale badly with the number of sequences. In this paper, we describe a new program called Clustal Omega, which can align virtually any number of protein sequences quickly and that delivers accurate alignments. The accuracy of the package on smaller test cases is similar to that of the high-quality aligners. On larger data sets, Clustal Omega outperforms other packages in terms of execution time and quality. Clustal Omega also has powerful features for adding sequences to and exploiting information in existing alignments, making use of the vast amount of precomputed information in public databases like Pfam.

", Publisher:="Sievers, F., et al.")>
<Cite(Title:="Fast, scalable generation of high-quality protein multiple sequence alignments using Clustal Omega",
      Volume:=7, Pages:="539",
      Authors:="Sievers, F.
Wilm, A.
Dineen, D.
Gibson, T. J.
Karplus, K.
Li, W.
Lopez, R.
McWilliam, H.
Remmert, M.
Soding, J.
Thompson, J. D.
Higgins, D. G.", Journal:="Molecular systems biology", ISSN:="1744-4292 (Electronic);
1744-4292 (Linking)", DOI:="10.1038/msb.2011.75", Year:=2011, Keywords:="Algorithms
Amino Acid Sequence
Base Sequence
Data Mining/*methods
Databases, Factual
Molecular Sequence Data
Proteins/*analysis/chemistry
Sequence Alignment/*methods
Sequence Analysis, Protein/*methods
Software
*Systems Biology/instrumentation/methods",
      Abstract:="Multiple sequence alignments are fundamental to many sequence analysis methods. Most alignments are computed using the progressive alignment heuristic. 
These methods are starting to become a bottleneck in some analysis pipelines when faced with data sets of the size of many thousands of sequences. 
Some methods allow computation of larger data sets while sacrificing quality, and others produce high-quality alignments, but scale badly with the number of sequences. 
In this paper, we describe a new program called Clustal Omega, which can align virtually any number of protein sequences quickly and that delivers accurate alignments. 
The accuracy of the package on smaller test cases is similar to that of the high-quality aligners. On larger data sets, Clustal Omega outperforms other packages in terms of execution time and quality. 
Clustal Omega also has powerful features for adding sequences to and exploiting information in existing alignments, making use of the vast amount of precomputed information in public databases like Pfam.",
      AuthorAddress:="School of Medicine and Medical Science, UCD Conway Institute of Biomolecular and Biomedical Research, University College Dublin, Dublin, Ireland.",
      PubMed:=21988835)>
Public Class Clustal : Inherits Microsoft.VisualBasic.CommandLine.InteropService

    Public Const CLUSTAL_ARGUMENTS As String = "--in ""{0}"" --out ""{1}"""

#Region "CLI"

#End Region

    ''' <summary>
    ''' 目标多序列比对文件的文件路径，出错会返回空值
    ''' </summary>
    ''' <param name="source"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function MultipleAlignment(source As String) As FASTA.FastaFile
        If Not FileIO.FileSystem.FileExists(source) Then
            Throw New DataException($"Source data file ""{source.ToFileURL}"" is not exists on your file system!")
        End If

        Dim out As String = App.GetAppSysTempFile(".fasta")
        Dim args As String = String.Format(CLUSTAL_ARGUMENTS, source, out)
        Call Console.WriteLine("EXEC --> {0} {1}", MyBase._executableAssembly, args)
        Call New CommandLine.IORedirect(MyBase._executableAssembly, args).Start(WaitForExit:=True)

        Dim result = FASTA.FastaFile.Read(out, False)
        Return result
    End Function

    Public Function Align(source As Generic.IEnumerable(Of SequenceModel.FASTA.FastaToken)) As FASTA.FastaFile
        Dim IO As New CommandLine.IORedirect(MyBase._executableAssembly, "")
        Dim fa As New SequenceModel.FASTA.FastaFile(source)
        Dim input As String = fa.Generate
        Call IO.Start(WaitForExit:=True, PushingData:={input})
        Dim out As String = IO.StandardOutput
        Return SequenceModel.FASTA.FastaFile.DocParser(out.lTokens)
    End Function

    Public Function AlignmentTask(source As String) As AsyncHandle(Of FASTA.FastaFile)
        Dim sourceInvoke = Function() MultipleAlignment(source)
        Dim hwnd As New AsyncHandle(Of FASTA.FastaFile)(sourceInvoke)
        Return hwnd.Run
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="Exe">Clustal可执行文件的文件路径</param>
    ''' <remarks></remarks>
    Sub New(Exe As String)
        MyBase._executableAssembly = Exe
    End Sub

    <ExportAPI("Session.New")>
    Public Shared Function CreateSession() As Clustal
        Dim DirList = ProgramPathSearchTool.SearchDirectory("clustal-omega", "")
        If DirList.IsNullOrEmpty Then
            GoTo RELEASE_PACKAGE
        End If
        For Each dir As String In DirList
            Dim Program = ProgramPathSearchTool.SearchProgram(dir, "clustalo")
            If Program.IsNullOrEmpty Then
                Continue For
            End If

            Return New ClustalOrg.Clustal(Program.First)
        Next

RELEASE_PACKAGE:
        Return New Clustal(ReleasePackage(App.AppSystemTemp))
    End Function

    <ExportAPI("clustal.align")>
    Public Shared Function Align(session As Clustal, source As String, argvs As String) As FASTA.FastaFile
        Return session.MultipleAlignment(source)
    End Function

    <ExportAPI("clustal.help")>
    Public Shared Function Help() As String
        Call Console.WriteLine(My.Resources.AUTHORS)
        Return My.Resources.AUTHORS
    End Function
End Class