﻿Imports System.Xml.Serialization
Imports LANS.SystemsBiology.AnalysisTools.SequenceTools.SequencePatterns.SequenceLogo
Imports LANS.SystemsBiology.Assembly.NCBI.GenBank.TabularFormat
Imports LANS.SystemsBiology.ComponentModel.Loci
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language

Namespace Motif

    ''' <summary>
    ''' A column in the motif
    ''' </summary>
    <XmlType("Residue")> Public Class ResidueSite : Implements IAddressHandle

        <XmlAttribute> Public Property Site As Integer Implements IAddressHandle.Address
        ''' <summary>
        ''' ATGC/
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property PWM As Double()
        ''' <summary>
        ''' Information content
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property Bits As Double

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region

        ''' <summary>
        ''' ATGC -> TACG
        ''' </summary>
        ''' <returns></returns>
        Public Function Complement() As ResidueSite
            Dim A As Double = PWM(0), T As Double = PWM(1), G As Double = PWM(2), C As Double = PWM(3)
            Dim cA As Double = T, cT As Double = A, cG As Double = C, cC As Double = G
            Dim rsd As New ResidueSite With {
                .PWM = {cA, cT, cG, cC},
                .Bits = Bits,
                .Site = Site
            }
            Return rsd
        End Function

        Sub New()
        End Sub

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="nt"></param>
        Sub New(nt As Char)
            If nt = "A"c OrElse nt = "a"c Then
                PWM = New Double() {1, 0, 0, 0}
            ElseIf nt = "T"c OrElse nt = "t"c Then
                PWM = New Double() {0, 1, 0, 0}
            ElseIf nt = "G"c OrElse nt = "g"c Then
                PWM = New Double() {0, 0, 1, 0}
            ElseIf nt = "C"c OrElse nt = "c"c Then
                PWM = New Double() {0, 0, 0, 1}
            Else  ' N, -, ., *
                PWM = New Double() {0.25, 0.25, 0.25, 0.25}
            End If

            Bits = 1.5
        End Sub

        Public Overrides Function ToString() As String
            Dim ATGC As String = New String({__toChar("A"c, PWM(0)), __toChar("T"c, PWM(1)), __toChar("G"c, PWM(2)), __toChar("C"c, PWM(3))})
            Return $"{ATGC}   //({Math.Round(Bits, 2)} bits) [{Math.Round(PWM(0), 2)}, {Math.Round(PWM(1), 2)}, {Math.Round(PWM(2), 2)}, {Math.Round(PWM(3), 2)}];"
        End Function

        Public ReadOnly Property AsChar As Char
            Get
                Dim mxInd As Integer = PWM.MaxIndex

                If PWM.Length = 4 Then
                    Return __toChar("ATGC"(mxInd), PWM(mxInd))
                Else
                    Return __toChar(ColorSchema.AA(mxInd), PWM(mxInd))
                End If
            End Get
        End Property

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="ch">大写的</param>
        ''' <param name="p"></param>
        ''' <returns></returns>
        Protected Friend Shared Function __toChar(ch As Char, p As Double) As Char
            If p < 0.65 Then
                ch = Char.ToLower(ch)
            End If
            Return ch
        End Function

        Public Shared Narrowing Operator CType(x As ResidueSite) As Double()
            Return x.PWM
        End Operator
    End Class
End Namespace