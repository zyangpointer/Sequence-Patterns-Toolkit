﻿Imports System.Drawing
Imports System.Linq
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Linq.Extensions

<XmlType("GSW", [Namespace]:="http://gcmodeller.org")>
Public Class Output

    ''' <summary>
    ''' best chain, 但是不明白这个有什么用途
    ''' </summary>
    ''' <returns></returns>
    Public Property Best As HSP

    ''' <summary>
    ''' 最佳的比对结果
    ''' </summary>
    ''' <returns></returns>
    Public Property HSP As HSP()
    ''' <summary>
    ''' Dynmaic programming matrix.(也可以看作为得分矩阵)
    ''' </summary>
    ''' <returns></returns>
    Public Property DP As Streams.Array.Double()
    ''' <summary>
    ''' The directions pointing to the cells that
    ''' give the maximum score at the current cell.
    ''' The first index is the column index.
    ''' The second index is the row index.
    ''' </summary>
    ''' <returns></returns>
    Public Property Directions As Streams.Array.Integer()

    Public Property Query As String
    Public Property Subject As String
    Public Property Traceback As Coords()

    Public Function ContainsPoint(i As Integer, j As Integer) As Boolean
        Dim LQuery = (From x In Traceback Where x.X = i AndAlso x.Y = j Select 100).FirstOrDefault
        Return LQuery > 50
    End Function

    Public Overrides Function ToString() As String
        Dim edits As String = ""
        Dim pre = Traceback.First

        For Each cd As Coords In Traceback.Skip(1)
            If cd.X - pre.X = -1 AndAlso cd.Y - pre.Y = -1 Then
                edits &= "m" '  match 和 substitute应该如何进行判断？？？
            End If
            If cd.X - pre.X = 0 AndAlso cd.Y - pre.Y = -1 Then
                edits &= "i"
            End If
            If cd.X - pre.X = -1 AndAlso cd.Y - pre.Y = 0 Then
                edits &= "d"
            End If

            pre = cd
        Next

        Return edits
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="sw"></param>
    ''' <param name="toChar"></param>
    ''' <param name="threshold">0% - 100%</param>
    ''' <returns></returns>
    Public Shared Function CreateObject(Of T)(sw As GSW(Of T), toChar As ToChar(Of T), threshold As Double, minW As Integer) As Output
        Dim best As HSP = Nothing
        Dim hsp = SequenceTools.SmithWaterman.HSP.CreateHSP(sw, toChar, best, cutoff:=threshold * sw.AlignmentScore)
        Dim direction = sw.prevCells.ToArray(Function(x) New Streams.Array.Integer(x))
        Dim dp = sw.GetDPMAT.ToArray(Function(x) New Streams.Array.Double(x))
        Dim query = New String(sw.query.ToArray(Function(x) toChar(x)))
        Dim subject = New String(sw.subject.ToArray(Function(x) toChar(x)))

        Dim m2Len As Integer = Math.Min(query.Length, subject.Length)
        If m2Len < minW Then
            Call $"Min width {minW} is too large than query/subject, using min(query,subject):={m2Len} instead....".__DEBUG_ECHO
            minW = m2Len
        End If
        hsp = (From x In hsp Where x.LengthHit >= minW AndAlso x.LengthQuery >= minW Select x).ToArray

        Return New Output With {
            .Traceback = sw.GetTraceback,
            .Directions = direction,
            .DP = dp,
            .HSP = hsp,
            .Query = query,
            .Subject = subject,
            .Best = best
        }
    End Function
End Class
