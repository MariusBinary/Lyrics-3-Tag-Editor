'Copyright (C) 2011-2012 Artem Los, www.clizware.net.
'The author of this code shall get the credits

' This project uses two general algorithms:
'  - Artem's Information Storage Format (Artem's ISF-2)
'  - Artem's Serial Key Algorithm (Artem's SKA-2)

' This project is also using an open source project, MegaMath
' that you might find here: http://megamath.sourceforge.net/
' MegaMath is license under the terms of th MIT License.

Imports System.Text
Imports System.Management
Imports System.Security

<Assembly: AllowPartiallyTrustedCallers()> 

#Region "S E R I A L  K E Y  G E N E R A T I N G  L I B R A R Y"

#Region "CONFIGURATION"
Public MustInherit Class BaseConfiguration
    'Put all functions/variables that should be shared with
    'all other classes that inherit this class.
    '
    'note, this class cannot be used as a normal class that
    'you define because it is MustInherit.

    Protected Friend _key As String = ""
    ''' <summary>
    ''' The key will be stored here
    ''' </summary>
    Public Overridable Property Key() As String
        'will be changed in both generating and validating classe.
        Get
            Return _key
        End Get
        Set(ByVal value As String)
            _key = value
        End Set
    End Property

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable ReadOnly Property MachineCode() As Integer

        Get
            Return getMachineCode()
        End Get

    End Property

    Private Shared Function getMachineCode() As String
        '      * Copyright (C) 2012 Artem Los, All rights reserved.
        '      * 
        '      * This code will generate a 5 digits long key, finger print, of the system
        '      * where this method is being executed. However, that might be changed in the
        '      * hash function "GetStableHash", by changing the amount of zeroes in
        '      * MUST_BE_LESS_OR_EQUAL_TO to the one you want to have. Ex 1000 will return 
        '      * 3 digits long hash.
        '      * 
        '      * Please note, that you might also adjust the order of these, but remember to
        '      * keep them there because as it is stated at 
        '      * (http://www.codeproject.com/Articles/17973/How-To-Get-Hardware-Information-CPU-ID-MainBoard-I)
        '      * the processorID might be the same at some machines, which will generate same
        '      * hashes for several machines.
        '      * 
        '      * The function will probably be implemented into SKGL Project at http://skgl.codeplex.com/
        '      * and Software Protector at http://softwareprotector.codeplex.com/, so I 
        '      * release this code under the same terms and conditions as stated here:
        '      * http://skgl.codeplex.com/license
        '      * 
        '      * Any questions, please contact me at
        '      *  * artem@artemlos.net
        '      
        Dim m As New methods

        Dim searcher As New ManagementObjectSearcher("select * from Win32_Processor")
        Dim collectedInfo As String = ""
        ' here we will put the informa
        For Each share As ManagementObject In searcher.[Get]()
            ' first of all, the processorid
            collectedInfo += share.GetPropertyValue("ProcessorId").ToString()
        Next

        searcher.Query = New ObjectQuery("select * from Win32_BIOS")
        For Each share As ManagementObject In searcher.[Get]()
            'then, the serial number of BIOS
            collectedInfo += share.GetPropertyValue("SerialNumber").ToString()
        Next

        searcher.Query = New ObjectQuery("select * from Win32_BaseBoard")
        For Each share As ManagementObject In searcher.[Get]()
            'finally, the serial number of motherboard
            collectedInfo += share.GetPropertyValue("SerialNumber").ToString()
        Next
        Return m.getEightByteHash(collectedInfo, 100000).ToString()
    End Function

End Class
Public Class SerialKeyConfiguration
    Inherits BaseConfiguration

#Region "V A R I A B L E S"
    Private _admBlock As Integer() = New Integer(0) {1}
    Public Property admBlock As Integer()
        Get
            Return _admBlock
        End Get
        Set(ByVal value As Integer())
            _admBlock = value
        End Set
    End Property
    Private _Features As Boolean() = New Boolean(7) {False, False, False, False, False, False, False, False} 'the default value of the Fetures array.
    Public Overridable Property Features As Boolean()
        'will be changed in validating class.
        Get
            Return _Features
        End Get
        Set(ByVal value As Boolean())
            _Features = value
        End Set
    End Property
    Private _addSplitChar As Boolean = True
    Public Property addSplitChar As Boolean
        Get
            Return _addSplitChar
        End Get
        Set(value As Boolean)
            _addSplitChar = value
        End Set
    End Property


#End Region

End Class
#End Region

#Region "ENCRYPTION"
Public Class Generate
    Inherits BaseConfiguration 'this class have to be inherited because of the key which is shared with both encryption/decryption classes.

    Dim skc As New SerialKeyConfiguration
    Dim m As New methods
    Dim r As New Random
    Public Sub New()
        ' No overloads works with Sub New
    End Sub
    Public Sub New(ByVal _serialKeyConfiguration As SerialKeyConfiguration)
        skc = _serialKeyConfiguration
    End Sub

    Private _secretPhase As String
    ''' <summary>
    ''' If the key is to be encrypted, enter a password here.
    ''' </summary>

    Public Property secretPhase() As String
        Get
            Return _secretPhase
        End Get
        Set(ByVal value As String)
            If value <> _secretPhase Then
                _secretPhase = m.twentyfiveByteHash(value)
            End If
        End Set
    End Property
    ''' <summary>
    ''' This function will generate a key.
    ''' </summary>
    ''' <param name="timeLeft">For instance, 30 days.</param>
    Public Function doKey(ByVal timeLeft As Integer) As String
        Return doKey(timeLeft, My.Computer.Clock.LocalTime, False)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="timeLeft">For instance, 30 days</param>
    ''' <param name="useMachineCode">Lock a serial key to a specific machine, given its "machine code". Should be 5 digits long.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function doKey(ByVal timeLeft As Integer, ByVal useMachineCode As Integer)
        Return doKey(timeLeft, My.Computer.Clock.LocalTime, useMachineCode)
    End Function

    ''' <summary>
    ''' This function will generate a key. You may also change the creation date.
    ''' </summary>
    ''' <param name="timeLeft">For instance, 30 days.</param>
    ''' <param name="creationDate">Change the creation date of a key.</param>
    ''' <param name="useMachineCode">Lock a serial key to a specific machine, given its "machine code". Should be 5 digits long.</param>
    Public Function doKey(ByVal timeLeft As Integer, ByVal creationDate As Date, Optional ByVal useMachineCode As Integer = 0) As String
        If timeLeft > 999 Then
            'Checking if the timeleft is NOT larger than 999. It cannot be larger to match the key-length 20.
            Throw New ArgumentException("The timeLeft is larger than 999. It can only consist of three digits.")
        End If

        If secretPhase <> "" Or secretPhase <> Nothing Then
            'if some kind of value is assigned to the variable "secretPhase", the code will execute it FIRST.
            'the secretPhase shall only consist of digits!
            Dim reg As New System.Text.RegularExpressions.Regex("^\d$") 'cheking the string
            If reg.IsMatch(secretPhase) Then
                'throwing new exception if the string contains non-numrical letters.
                Throw New ArgumentException("The secretPhase consist of non-numerical letters.")
            End If
        End If

        'if no exception is thown, do following
        Dim _stageThree As String
        If useMachineCode > 0 And useMachineCode <= 99999 Then
            _stageThree = m._encrypt(timeLeft, skc.Features, secretPhase, useMachineCode, creationDate) ' stage one
        Else
            _stageThree = m._encrypt(timeLeft, skc.Features, secretPhase, r.Next(0, 99999), creationDate) ' stage one
        End If

        'if it is the same value as default, we do not need to mix chars. This step saves generation time.
        If skc.admBlock.Count = 20 Then

            If skc.addSplitChar = True Then
                ' by default, a split character will be added
                Key = m.setMixChars(_stageThree, skc.admBlock)
                _stageThree = Key.Substring(0, 5) + "-" + Key.Substring(5, 5) + "-" + Key.Substring(10, 5) + "-" + Key.Substring(15, 5)
                Key = _stageThree
                Return Key
            Else
                'we also include the key in the Key variable to make it possible for user to get his key without generating a new one.
                Key = m.setMixChars(_stageThree, skc.admBlock)
                Return Key
            End If
        Else
            If skc.addSplitChar = True Then
                ' by default, a split character will be addedr
                Key = _stageThree.Substring(0, 5) + "-" + _stageThree.Substring(5, 5) + "-" + _stageThree.Substring(10, 5) + "-" + _stageThree.Substring(15, 5)
            Else
                Key = _stageThree
            End If

            'we also include the key in the Key variable to make it possible for user to get his key without generating a new one.
            Return Key

        End If

    End Function
End Class
#End Region

#Region "DECRYPTION"
Public Class Validate
    Inherits BaseConfiguration 'this class have to be inherited becuase of the key which is shared with both encryption/decryption classes.

    Dim skc As New SerialKeyConfiguration
    Dim _a As New methods
    Public Sub New()
        ' No overloads works with Sub New
    End Sub
    Public Sub New(ByVal _serialKeyConfiguration As SerialKeyConfiguration)
        skc = _serialKeyConfiguration
    End Sub
    ''' <summary>
    ''' Enter a key here before validating.
    ''' </summary>
    Public Overloads Property Key As String 're-defining the Key
        Get
            Return _key
        End Get
        Set(ByVal value As String)
            _res = ""
            _key = value
        End Set
    End Property

    Private _secretPhase As String = ""
    ''' <summary>
    ''' If the key has been encrypted, when it was generated, please set the same secretPhase here.
    ''' </summary>
    Public Property secretPhase() As String
        Get
            Return _secretPhase
        End Get
        Set(ByVal value As String)
            If value <> _secretPhase Then
                _secretPhase = _a.twentyfiveByteHash(value)
            End If
        End Set
    End Property

    Private _res As String = ""

    Private Sub decodeKeyToString()

        If _res = "" Or _res = Nothing Then ' checking if the key already have been decoded.

            Dim _stageOne As String = ""

            Key = Key.Replace("-", "")

            'if the admBlock has been changed, the getMixChars will be executed.
            If skc.admBlock.Count = 20 Then

                _stageOne = _a.getMixChars(Key, skc.admBlock)
            Else

                _stageOne = Key
            End If

            _stageOne = Key

            ' _stageTwo = _a._decode(_stageOne)

            If secretPhase <> "" Or secretPhase <> Nothing Then
                'if no value "secretPhase" given, the code will directly decrypt without using somekind of encryption
                'if some kind of value is assigned to the variable "secretPhase", the code will execute it FIRST.
                'the secretPhase shall only consist of digits!
                Dim reg As New System.Text.RegularExpressions.Regex("^\d$") 'cheking the string
                If reg.IsMatch(secretPhase) Then
                    'throwing new exception if the string contains non-numrical letters.
                    Throw New ArgumentException("The secretPhase consist of non-numerical letters.")
                End If
            End If
            _res = _a._decrypt(_stageOne, secretPhase)


        End If
    End Sub
    Private Function _IsValid() As Boolean
        'Dim _a As New methods ' is only here to provide the geteighthashcode method
        Try
            If Key.Contains("-") Then
                If Key.Length <> 23 Then
                    Return False
                End If
            Else
                If Key.Length <> 20 Then
                    Return False
                End If
            End If
            decodeKeyToString()

            Dim _decodedHash As String = _res.Substring(0, 9)
            Dim _calculatedHash As String = _a.getEightByteHash(_res.Substring(9, 19)).ToString.Substring(0, 9) ' changed Math.Abs(_res.Substring(0, 17).GetHashCode).ToString.Substring(0, 8)

            'When the hashcode is calculated, it cannot be taken for sure, 
            'that the same hash value will be generated.
            'learn more about this issue: http://msdn.microsoft.com/en-us/library/system.object.gethashcode.aspx
            If _decodedHash = _calculatedHash Then
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            'if something goes wrong, for example, when decrypting, 
            'this function will return false, so that user knows that it is unvalid.
            'if the key is valid, there won't be any errors.
            Return False
        End Try
    End Function
    ''' <summary>
    ''' Checks whether the key has been modified or not. If the key has been modified - returns false; if the key has not been modified - returns true.
    ''' </summary>
    Public ReadOnly Property IsValid As Boolean
        Get
            Return _IsValid()
        End Get
    End Property
    Private Function _IsExpired() As Boolean
        If DaysLeft > 0 Then
            Return False
        Else
            Return True
        End If
    End Function
    ''' <summary>
    ''' If the key has expired - returns true; if the key has not expired - returns false.
    ''' </summary>
    Public ReadOnly Property IsExpired As Boolean
        Get
            Return _IsExpired()
        End Get
    End Property
    Private Function _CreationDay() As Date
        decodeKeyToString()
        Dim _date As New Date
        _date = _res.Substring(9, 4) & "-" & _res.Substring(13, 2) & "-" & _res.Substring(15, 2)

        Return _date
    End Function
    ''' <summary>
    ''' Returns the creation date of the key.
    ''' </summary>
    Public ReadOnly Property CreationDate() As Date
        Get
            Return _CreationDay()
        End Get
    End Property
    Private Function _DaysLeft() As Integer
        decodeKeyToString()
        Dim _setDays As Integer = SetTime
        Return DateDiff(DateInterval.DayOfYear, Today, ExpireDate)
    End Function
    ''' <summary>
    ''' Returns the amount of days the key will be valid.
    ''' </summary>
    Public ReadOnly Property DaysLeft() As Integer
        Get
            Return _DaysLeft()
        End Get
    End Property

    Private Function _SetTime() As Integer
        decodeKeyToString()
        Return _res.Substring(17, 3)
    End Function
    ''' <summary>
    ''' Returns the actual amount of days that were set when the key was generated.
    ''' </summary>
    Public ReadOnly Property SetTime() As Integer
        Get
            Return _SetTime()
        End Get
    End Property
    Private Function _ExpireDate() As Date
        decodeKeyToString()
        Dim _date As New Date
        _date = CreationDate
        Return _date.AddDays(SetTime)
    End Function
    ''' <summary>
    ''' Returns the date when the key is to be expired.
    ''' </summary>
    Public ReadOnly Property ExpireDate As Date
        Get
            Return _ExpireDate()
        End Get
    End Property
    Private Function _Features() As Boolean()
        decodeKeyToString()
        Return _a.intToBoolean(_res.Substring(20, 3))
    End Function
    ''' <summary>
    ''' Returns all 8 features in a boolean array
    ''' </summary>
    Public Overloads ReadOnly Property Features As Boolean()
        'we already have defined Features in the BaseConfiguration class. 
        'Here we only change it to Read Only.
        Get
            Return _Features()
        End Get
    End Property

    ''' <summary>
    ''' If the current machine's machine code is equal to the one that this key is designed for, return true.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property IsOnRightMachine() As Boolean
        Get
            Dim decodedMachineCode As Integer = _res.Substring(23, 5)

            Return decodedMachineCode = MachineCode
        End Get
    End Property
End Class
#End Region

#Region "T H E  C O R E  O F  S K G L"
Friend Class methods

    Inherits SerialKeyConfiguration

    'The construction of the key
    Protected Friend Function _encrypt(ByVal _days As Integer, ByVal _tfg() As Boolean, ByVal _secretPhase As String, ByVal ID As Integer, ByVal _creationDate As Date) As String
        ' This function will store information in Artem's ISF-2
        'Random variable was moved because of the same key generation at the same time.

        Dim _retInt As Integer = _creationDate.ToString("yyyyMMdd") ' today

        Dim result As Decimal = 0

        result += _retInt ' adding the current date; the generation date; today.
        result *= 1000    ' shifting three times at left

        result += _days   ' adding time left
        result *= 1000    ' shifting three times at left

        result += booleanToInt(_tfg) ' adding features
        result *= 100000    'shifting three times at left

        result += ID ' adding random ID

        ' This part of the function uses Artem's SKA-2

        If _secretPhase = "" Or _secretPhase = Nothing Then
            ' if not password is set, return an unencrypted key
            Return base10ToBase26(getEightByteHash(result) & result)
        Else
            ' if password is set, return an encrypted 
            Return base10ToBase26(getEightByteHash(result) & _encText(result, _secretPhase))
        End If


    End Function
    Protected Friend Function _decrypt(ByVal _key As String, ByVal _secretPhase As String) As String
        If _secretPhase = "" Or _secretPhase = Nothing Then
            ' if not password is set, return an unencrypted key
            Return base26ToBase10(_key)
        Else
            ' if password is set, return an encrypted 
            Dim usefulInformation As String = base26ToBase10(_key)
            Return usefulInformation.Substring(0, 9) & _decText(usefulInformation.Substring(9), _secretPhase)
        End If

    End Function
    'Deeper - encoding, decoding, et cetera.

    'Convertions, et cetera.----------------
    Protected Friend Function setMixChars(ByVal _text() As Char, ByVal _admBlock() As Integer)
        Dim _newText As String = ""
        For i As Integer = 0 To _text.Length - 1
            _newText &= _text(_admBlock(i))
        Next
        Return _newText
    End Function
    Protected Friend Function getMixChars(ByVal _text() As Char, ByVal _admBlock() As Integer)
        Dim _newText As String = ""
        For i As Integer = 0 To _text.Length - 1
            _newText &= _text(New ArrayList(_admBlock).IndexOf(i))
        Next
        Return _newText
    End Function
    Protected Friend Function booleanToInt(ByVal _booleanArray() As Boolean) As Integer
        Dim _aVector As Integer = 0 '
        'In this function we are converting a binary value array to a int
        'A binary array can max contain 4 values.
        'Ex: new boolean(){1,1,1,1}

        For _i As Integer = 0 To _booleanArray.Length - 1
            Select Case _booleanArray(_i)
                Case -1
                    _aVector += (2 ^ (_booleanArray.Length - _i - 1)) ' times 1 has been removed
            End Select
        Next
        Return _aVector
    End Function
    Protected Friend Function intToBoolean(ByVal _num As Integer) As Boolean()
        'In this function we are converting an integer (created with privious function) to a binary array

        Dim _bReturn As Integer = Convert.ToString(_num, 2)
        Dim _aReturn As String = Return_Lenght(_bReturn, 8)
        Dim _cReturn(7) As Boolean

        For i As Integer = 0 To 7

            _cReturn(i) = _aReturn.ToString.Substring(i, 1)
        Next
        Return _cReturn
    End Function
    Protected Friend Function _encText(ByVal _inputPhase As String, ByVal _secretPhase As String) As String
        'in this class we are encrypting the integer array.
        Dim _res As String

        For i As Integer = 0 To _inputPhase.Length - 1
            _res &= modulo(_inputPhase.Substring(i, 1) + +_secretPhase.Substring(modulo(i, _secretPhase.Length), 1), 10)
        Next

        Return _res
    End Function
    Protected Friend Function _decText(ByVal _encryptedPhase As String, ByVal _secretPhase As String) As String
        'in this class we are decrypting the text encrypted with the function above.
        Dim _res As String

        For i As Integer = 0 To _encryptedPhase.Length - 1
            _res &= modulo(_encryptedPhase.Substring(i, 1) - _secretPhase.Substring(modulo(i, _secretPhase.Length), 1), 10)
        Next

        Return _res
    End Function
    Protected Friend Function Return_Lenght(ByVal Number As String, ByVal Lenght As Integer) As String
        ' This function create 3 lenght char ex: 39 to 039
        If (Number.ToString.Length <> Lenght) Then
            While Not (Number.ToString.Length = Lenght)
                Number = "0" & Number
            End While
        End If
        Return Number 'Return Number

    End Function
    Protected Friend Function modulo(ByVal _num As Integer, ByVal _base As Integer) As Integer ' canged return type to integer.
        'this function simply calculates the "right modulo".
        'by using this function, there won't, hopefully be a negative
        'number in the result!
        Return _num - _base * Int(_num / _base)
    End Function
    Protected Friend Function twentyfiveByteHash(ByVal s As String) As String
        Dim amountOfBlocks As Integer = s.Length / 5
        Dim preHash As String() = New String(amountOfBlocks) {}

        If s.Length <= 5 Then
            'if the input string is shorter than 5, no need of blocks! 
            preHash(0) = getEightByteHash(s).ToString()
        ElseIf s.Length > 5 Then
            'if the input is more than 5, there is a need of dividing it into blocks.
            For i As Integer = 0 To amountOfBlocks - 2
                preHash(i) = getEightByteHash(s.Substring(i * 5, 5)).ToString()
            Next

            preHash(preHash.Length - 2) = getEightByteHash(s.Substring((preHash.Length - 2) * 5, s.Length - (preHash.Length - 2) * 5)).ToString()
        End If
        Return String.Join("", preHash)
    End Function
    Protected Friend Function getEightByteHash(ByVal s As String, Optional MUST_BE_LESS_THAN As Integer = 1000000000) As Integer
        'This function generates a eight byte hash

        'The length of the result might be changed to any length
        'just set the amount of zeroes in MUST_BE_LESS_THAN
        'to any length you want

        Dim hash As UInteger = 0

        For Each b As Byte In System.Text.Encoding.Unicode.GetBytes(s)
            hash += b
            hash += (hash << 10)
            hash = hash Xor (hash >> 6)
        Next

        hash += (hash << 3)
        hash = hash Xor (hash >> 11)
        hash += (hash << 15)

        Dim result As UInteger = hash Mod MUST_BE_LESS_THAN
        Dim check As Integer = Fix(MUST_BE_LESS_THAN / result)

        If check > 1 Then
            'checking so that all results are of the same size
            result *= check
        End If

        Return result
    End Function
    Protected Friend Function base10ToBase26(ByVal s As Decimal) As String
        ' This method is converting a base 10 number to base 26 number.
        ' Remember that s is a decimal, and the size is limited. 
        ' In order to get size, type Decimal.MaxValue.
        '
        ' Note that this method will still work, even though you only 
        ' can add, subtract numbers in range of 15 digits.
        Dim allowedLetters As Char() = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray()

        Dim num As Decimal = s
        Dim reminder As Integer

        Dim result(s.ToString.Length) As Char
        Dim j As Integer = 0


        While (num >= 26)
            reminder = num Mod 26
            result(j) = allowedLetters(reminder)
            num = (num - reminder) / 26
            j += 1
        End While

        result(j) = allowedLetters(num) ' final calculation

        Dim returnNum As String = ""
        For k As Integer = j To k < 0 Step -1
            returnNum &= result(k)
        Next
        Return returnNum

    End Function
    Protected Friend Function base26ToBase10(ByVal s As String) As Decimal
        ' This function will convert a number that has been generated
        ' with functin above, and get the actual number in decimal
        '
        ' This function requieres Mega Math to work correctly.

        Dim allowedLetters As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
        Dim result As Decimal

        For i As Integer = 0 To s.Length - 1 Step 1

            Dim pow As String = powof("26", (s.Length - i - 1).ToString).ToString

            result = MegaMath.MegaIntegerAddition(result, MegaMath.MegaIntgerMultiply(allowedLetters.IndexOf(s.Substring(i, 1)).ToString, pow))

        Next i

        Return result
    End Function

    Protected Friend Function powof(x As String, y As String) As String
        ' Because of the uncertain answer using Math.Pow and ^, 
        ' this function is here to solve that issue.
        ' It is currently using the MegaMath library to calculate.
        Dim newNum As String = 1
        Dim megacalc As New MegaMath

        If y = "0" Then
            Return 1 ' if 0, return 1, e.g. x^0 = 1 (mathematicaly proven!) 
        ElseIf y = "1" Then
            Return x ' if 1, return x, which is the base, e.g. x^1 = x
        Else
            For i As Integer = 0 To y - 1
                newNum = MegaMath.MegaIntgerMultiply(newNum.ToString, x)
            Next
            Return newNum ' if both conditions are not satisfied, this loop
            ' will continue to y, which is the exponent.
        End If
    End Function
End Class


#End Region

#Region "MEGA MATH"

Friend Class MegaMath

    Public ex_args As New Exception("Invalid parameter")

#Region "Integer Functions"

    ''' <summary>
    ''' Divide one large integer by another
    ''' </summary>
    ''' <param name="value">Number</param>
    ''' <param name="divisor">Divisor</param>
    ''' <param name="sModulus">Pass a string to this parameter to receive the remainder</param>
    ''' <returns>Signed integer</returns>
    ''' <remarks></remarks>
    Public Shared Function MegaIntegerDivide(ByRef value As String, ByRef divisor As String, Optional ByRef sModulus As String = "") As String
        Dim bNegative As Boolean = False

        If isPositive(value) AndAlso isNegative(divisor) Then
            bNegative = True
        ElseIf isNegative(value) AndAlso isPositive(divisor) Then
            bNegative = True
        End If

        If value = divisor Then
            sModulus = 0
            Return "1"
        ElseIf ReturnLargerInteger(value, divisor) = divisor Then
            sModulus = value
            Return "0"
        End If

        Dim run As String = "0"
        Dim sb As New StringBuilder

        For Each item As String In value
            run = ScrubLeadingZeros(run & item)

            If run = "0" OrElse run = Nothing Then
                run = "0"
                sb.Append("0")
                Continue For
            End If

            If ReturnLargerInteger(divisor, run) = run Then
                Dim s As String = String.Empty
                Dim p As String = MegaIntegerSimpleDivide(run, divisor, s)
                sb.Append(p)
                run = s
            End If
        Next

        sModulus = run
        If bNegative Then
            Return MegaNegAbsolute(sb.ToString)
        Else
            Return sb.ToString
        End If

    End Function

    ''' <summary>
    ''' Multiply two very large numbers
    ''' </summary>
    ''' <param name="item1">number 1</param>
    ''' <param name="item2">number 2</param>
    ''' <returns>Product of multiplication</returns>
    ''' <remarks></remarks>
    Public Shared Function MegaIntgerMultiply(ByRef item1 As String, ByRef item2 As String) As String
        Dim bNeg As Boolean = False
        If isNegative(item1) AndAlso isPositive(item2) Then
            bNeg = True
        ElseIf isPositive(item1) AndAlso isNegative(item2) Then
            bNeg = True
        End If

        Dim strTop As String = MegaAbsolute(ReturnLargerInteger(item1, item2))
        Dim strBottom As String = MegaAbsolute(ReturnSmallerInteger(item1, item2))

        Dim pList As New ArrayList
        Dim xCounter As String = "0"

        For Each nBottom In strBottom.Reverse
            Dim sb As New StringBuilder
            Dim carry As Int16 = 0

            Dim i As String = "0"
            Do Until i = xCounter
                sb.Insert(0, "0", 1)
                i = MegaIntegerAddition(i, "1")
            Loop

            For Each nTop In strTop.Reverse
                Dim x As String = nTop.ToString
                Dim y As String = nBottom.ToString

                Dim nresult As Int32 = Convert.ToInt16(x) * Convert.ToInt16(y)

                nresult += carry
                carry = 0

                Dim sresult As String = nresult.ToString
                Dim lresult As Int32 = sresult.Length

                Dim drop As String = sresult.Substring(lresult - 1)
                Dim scarry As String = sresult.Substring(0, lresult - 1)

                If Not scarry = Nothing Then
                    carry = Convert.ToInt16(scarry)
                End If

                sb.Insert(0, drop.ToString, 1)
            Next
            If carry > 0 Then sb.Insert(0, carry.ToString, 1)
            pList.Add(sb.ToString)
            xCounter = MegaIntegerAddition(xCounter, "1")
        Next

        Dim product As String = "0"

        For Each lItem As String In pList
            product = MegaIntegerAddition(product, lItem)
        Next

        If bNeg Then
            Return MegaNegAbsolute(product)
        Else
            Return product
        End If


    End Function

    ''' <summary>
    ''' Add two very large numbers
    ''' </summary>
    ''' <param name="item1"></param>
    ''' <param name="item2"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function MegaIntegerAddition(ByRef item1 As String, ByRef item2 As String) As String
        Dim negresult As Boolean = False

        If isNegative(item1) AndAlso isNegative(item2) Then
            negresult = True
        ElseIf isNegative(item1) AndAlso isPositive(item2) Then
            If MegaAbsolute(item1) = item2 Then
                Return "0" 'items cancel out
            End If
            If ReturnLargerInteger(item2, item1) = item2 Then
                Return MegaIntegerSubtraction(item2, MegaAbsolute(item1))
            Else
                Return MegaIntegerSubtraction(MegaAbsolute(item1), item2)
            End If
        ElseIf isNegative(item2) AndAlso isPositive(item1) Then
            If item1 = MegaAbsolute(item2) Then
                Return "0" 'items cancel out
            End If
            If ReturnLargerInteger(item2, item1) = item1 Then
                Return MegaIntegerSubtraction(item1, MegaAbsolute(item2))
            Else
                Return MegaNegAbsolute(MegaIntegerSubtraction(item1, MegaAbsolute(item2)))
            End If
        End If

        Dim aItem1 As String = MegaAbsolute(item1)
        Dim aItem2 As String = MegaAbsolute(item2)

        Dim sb As New StringBuilder
        Dim carry As Int16 = 0
        Dim x As Long = 0

        Do Until x = aItem1.Length Or x = aItem2.Length
            Dim y As Int16 = 0
            Dim z As Int16 = 0
            Dim sum As Int16 = 0

            Try
                y = Convert.ToInt16(aItem1.Substring(aItem1.Length - x - 1, 1))
                z = Convert.ToInt16(aItem2.Substring(aItem2.Length - x - 1, 1))
            Catch ex As Exception
                Throw ex
            End Try

            sum = y + z

            If carry > 0 Then
                sum += carry
            End If

            If sum < 10 Then
                sb.Insert(0, sum, 1)
                carry = 0
            ElseIf sum >= 10 AndAlso sum <= 19 Then
                sb.Insert(0, sum - 10, 1)
                carry = 1
            End If

            x += 1
        Loop

        If aItem1.Length <> aItem2.Length Then
            Dim sLong As String = String.Empty
            Dim sShort As String = String.Empty

            If aItem1.Length > aItem2.Length Then
                sLong = aItem1
                sShort = aItem2
            Else
                sLong = aItem2
                sShort = aItem1
            End If

            Dim diff As Int64 = sLong.Length - sShort.Length

            If carry > 0 Then
                Dim temp As String = sLong.Substring(0, diff)
                temp = MegaIntegerAddition(temp, carry)
                sb.Insert(0, temp, 1)
            Else
                Dim ext As String = sLong.Substring(0, diff)
                sb.Insert(0, ext, 1)
            End If
        Else
            If carry > 0 Then
                sb.Insert(0, carry, 1)
            End If
        End If

        If negresult Then
            Return "-" & sb.ToString
        Else
            Return sb.ToString
        End If

    End Function

    ''' <summary>
    ''' Subtract two very large numbers (item1 - item2)
    ''' </summary>
    ''' <param name="item1">item1</param>
    ''' <param name="item2">item2</param>
    ''' <returns>Signed integer</returns>
    ''' <remarks></remarks>
    Public Shared Function MegaIntegerSubtraction(ByRef item1 As String, ByRef item2 As String) As String
        Dim bNegResult As Boolean = False

        If isNegative(item1) AndAlso isPositive(item2) Then 'if oposite sign and same value
            If MegaAbsolute(item1) = MegaAbsolute(item2) Then
                Return "0"
            End If
            Return MegaIntegerAddition(MegaNegAbsolute(item1), MegaNegAbsolute(item2))
        ElseIf isPositive(item1) AndAlso isNegative(item2) Then
            Return MegaIntegerAddition(item1, item2)

        ElseIf isNegative(item1) AndAlso isNegative(item2) Then
            If item1 = item2 Then Return "0"
            If ReturnLargerInteger(item1, item2) = item2 Then
                bNegResult = False
            Else
                bNegResult = True
            End If

        ElseIf isPositive(item1) AndAlso isPositive(item2) Then
            If item1 = item2 Then Return "0"
            If ReturnLargerInteger(item1, item2) = item2 Then
                bNegResult = True
            End If

        End If

        Dim sb As New StringBuilder
        Dim carry As Int16 = 0
        Dim top As String = MegaAbsolute(ReturnLargerInteger(item2, item1))
        Dim bottom As String = MegaAbsolute(ReturnSmallerInteger(item2, item1))

        Do 'even out bottom with leading 0s
            If top.Length = bottom.Length Then Exit Do
            bottom = "0" & bottom
        Loop

        Dim x As Long = 0

        Do Until x = top.Length
            Dim y As Int16 = 0
            Dim z As Int16 = 0
            Dim subtract As Int16 = 0
            Dim drop As Int16 = 0
            Try
                y = Convert.ToInt16(top.Substring(top.Length - x - 1, 1))
                z = Convert.ToInt16(bottom.Substring(bottom.Length - x - 1, 1))
            Catch ex As Exception
                Throw ex
            End Try

            If carry > 0 Then
                y -= carry
            End If

            If z > y Then 'must carry from next power
                carry = 1
                drop = y + 10 - z
            Else
                carry = 0
                drop = y - z
            End If

            If drop > 10 Then
                'mystery
            End If

            sb.Insert(0, drop, 1)

            x += 1
        Loop
        If bNegResult Then
            Return ScrubLeadingZeros("-" & sb.ToString)
        Else
            Return ScrubLeadingZeros(sb.ToString)
        End If


    End Function

#End Region

#Region "Helper functions"

    Protected Shared Function MegaNegAbsolute(ByRef num As String) As String
        If num.StartsWith("-") Then
            Return num
        End If
        Return "-" & num
    End Function

    Protected Shared Function MegaAbsolute(ByRef num As String) As String
        If num.StartsWith("-") Then
            num = num.Substring(1)
        End If
        Return num
    End Function

    Protected Shared Function isPositive(ByRef num As String) As Boolean
        If num.StartsWith("-") Then Return False
        Return True
    End Function

    Protected Shared Function isNegative(ByRef num As String) As Boolean
        If num.StartsWith("-") Then Return True
        Return False
    End Function

    Protected Shared Function MegaIntegerSimpleDivide(ByRef value As String, ByRef divisor As String, Optional ByRef modulus As String = "") As String
        Dim dividend As String = String.Empty
        Dim count As String = "1"
        modulus = "0"

        Do
            dividend = MegaIntgerMultiply(count, divisor)

            If dividend = value Then
                Return count
                modulus = 0
            ElseIf ReturnLargerInteger(dividend, value) = dividend Then 'end the loop
                Dim product As String = MegaIntegerSubtraction(count, "1")
                Dim z As String = MegaIntgerMultiply(product, divisor)
                modulus = ScrubLeadingZeros(MegaIntegerSubtraction(value, z))
                Return product
            End If

            count = MegaIntegerAddition(count, "1")
        Loop

    End Function

    Protected Shared Function ScrubLeadingZeros(ByRef value As String) As String
        Do
            If value.StartsWith("0") Then
                value = value.Substring(1)
            Else
                Exit Do
            End If
        Loop
        Return value
    End Function

#End Region

#Region "Other"

    Public Shared Function ReturnLargerInteger(ByRef item1 As String, ByRef item2 As String) As String


        Dim aItem1 As String = MegaAbsolute(item1)
        Dim aItem2 As String = MegaAbsolute(item2)

        If aItem1.Length > aItem2.Length Then
            Return item1
        End If

        If aItem2.Length > aItem1.Length Then
            Return item2
        End If

        Dim x As Int64 = 0

        Do Until x = aItem1.Length
            Dim y As Int16 = 0
            Dim z As Int16 = 0
            y = Convert.ToInt16(aItem1.Substring(x, 1))
            z = Convert.ToInt16(aItem2.Substring(x, 1))

            If y > z Then
                Return item1
            End If

            If y < z Then
                Return item2
            End If

            x += 1
        Loop
        Return item1
    End Function

    Public Shared Function ReturnSmallerInteger(ByRef item1 As String, ByRef item2 As String) As String
        If item1.Length > item2.Length Then
            Return item2
        End If

        If item2.Length > item1.Length Then
            Return item1
        End If

        Dim x As Int64 = 0

        Do Until x = item1.Length
            Dim y As Int16 = 0
            Dim z As Int16 = 0
            y = Convert.ToInt16(item1.Substring(x, 1))
            z = Convert.ToInt16(item2.Substring(x, 1))

            If y > z Then
                Return item2
            End If

            If y < z Then
                Return item1
            End If

            x += 1
        Loop
        Return item1 'items should be the same if execution reaches this point...
    End Function

    ''' <summary>
    ''' Calculate the Collatz trajectory for a given number
    ''' </summary>
    ''' <param name="num">Integer to calculate</param>
    ''' <remarks></remarks>
    Public Shared Sub Collatz(ByRef num As String)
        Dim jumps As String = "0"

        Dim cal As String = String.Empty
        cal = num
        Do
            If cal.EndsWith("2") OrElse cal.EndsWith("4") Or cal.EndsWith("6") Or cal.EndsWith("8") Or cal.EndsWith("0") Then
                cal = MegaIntegerDivide(cal, "2")
            Else
                cal = MegaIntgerMultiply(cal, "3")
                cal = MegaIntegerAddition(cal, "1")
            End If
            Console.WriteLine(cal)
            jumps = MegaIntegerAddition(jumps, "1")
            If cal = "1" Then Exit Do
        Loop
        Console.WriteLine()
        Console.WriteLine("Jumps: " & jumps)
    End Sub

#End Region

End Class
#End Region

#End Region