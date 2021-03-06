﻿Imports LANS.SystemsBiology.SequenceModel.FASTA
Imports LANS.SystemsBiology.SequenceModel.FASTA.Reflection
Imports LANS.SystemsBiology.SequenceModel
Imports LANS.SystemsBiology.AnalysisTools.SequenceTools.SequencePatterns.Pattern
Imports LANS.SystemsBiology.AnalysisTools.SequenceTools.SequencePatterns
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CommandLine.Reflection

<[PackageNamespace]("SequenceTools",
                    Category:=APICategories.ResearchTools,
                    Description:="Sequence search tools and sequence operation tools",
                    Publisher:="xie.guigang@gmail.com")>
Public Module ShellScriptAPI

    <ExportAPI("search.title_keyword")>
    Public Function SearchByTitleKeyword(fasta As FastaFile, Keyword As String) As FastaFile
        Dim LQuery As FastaToken() =
            LinqAPI.Exec(Of FastaToken) <= From fa As FastaToken
                                           In fasta
                                           Where InStr(fa.Title, Keyword, CompareMethod.Binary) > 0
                                           Select fa
        Return LQuery
    End Function

    <ExportAPI("reverse")>
    Public Function Reverse(fasta As FastaFile) As FastaFile
        Return fasta.Reverse
    End Function

    <ExportAPI("reverse")>
    Public Function Reverse(fasta As FastaToken) As FastaFile
        Return fasta.Reverse
    End Function

    <ExportAPI("Read.Fasta")>
    Public Function ReadFile(file As String) As FastaFile
        Return FastaFile.Read(file)
    End Function

    <ExportAPI("write.fasta")>
    Public Function WriteFile(fasta As FastaFile, file As String) As Boolean
        Return fasta.Save(file)
    End Function

    <ExportAPI("get_fasta")>
    Public Function GetObject(fasta As FastaFile, index As Integer) As FastaToken
        Return fasta.Item(index)
    End Function

    <ExportAPI("get_sequence")>
    Public Function GetSequenceData(fsa As FastaToken) As String
        Return fsa.SequenceData
    End Function

    ''' <summary>
    ''' 使用正则表达式搜索目标序列
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <ExportAPI("-Pattern_Search", Info:="Parsing the sequence segment from the sequence source using regular expression.",
        Usage:="-pattern_search fasta <fasta_object> pattern <regex_pattern> output <output_directory>")>
    <ParameterInfo("-p",
        Description:="This switch specific the regular expression pattern for search the sequence segment,\n" &
                     "for more detail information about the regular expression please read the user manual.",
        Example:="N{1,5}TA")>
    <ParameterInfo("-o", True,
        Description:="Optional, this switch value specific the output directory for the result data, default is user Desktop folder.",
        Example:="~/Documents/")>
    Public Function PatternSearchA(fasta As FastaFile, pattern As String, outDIR As String) As Integer
        pattern = pattern.Replace("N", "[ATGCU]")

        If String.IsNullOrEmpty(outDIR) Then
            outDIR = App.Desktop
        End If

        Dim Csv = SequencePatterns.Pattern.Match(Seq:=fasta, pattern:=pattern)
        Dim Complement = SequencePatterns.Pattern.Match(Seq:=fasta.Complement(), pattern:=pattern)
        Dim Reverse = SequencePatterns.Pattern.Match(Seq:=fasta.Reverse, pattern:=pattern)

        Call Csv.Insert(rowId:=-1, Row:={"Match pattern:=", pattern})
        Call Complement.Insert(rowId:=-1, Row:={"Match pattern:=", pattern})
        Call Reverse.Insert(rowId:=-1, Row:={"Match pattern:=", pattern})

        Call Csv.Save(outDIR & "/sequence.csv", False)
        Call Complement.Save(outDIR & "/sequence_complement.csv", False)
        Call Reverse.Save(outDIR & "/sequence_reversed.csv", False)

        Return 0
    End Function

    <ExportAPI("Search")>
    Public Function PatternSearch(Fasta As FastaToken, Pattern As String) As SegLoci()
        Throw New NotImplementedException
    End Function

    <ExportAPI("loci.match.location")>
    Public Function MatchLocation(Sequence As String, Loci As String, Optional cutoff As Double = 0.65) As Topologically.SimilarityMatches.LociMatchedResult()
        Return Topologically.SimilarityMatches.MatchLociLocations(Sequence, Loci, Len(Loci) / 3, Len(Loci) * 5, cutoff)
    End Function

    <ExportAPI("Align")>
    Public Function Align(query As FastaToken, subject As FastaToken, Optional cost As Double = 0.7) As AlignmentResult
        Return New AlignmentResult(query, subject, cost)
    End Function
End Module
