# 6.0 Standard Library

## 6.1 VBA Project

The `VBA` project is a _host project_ that is present in every _VBA environment_. The `VBA` project consists of a set of classes, functions, `Enum` and constants that form VBA's _standard library_.

🎯 The **RDCore** platform must therefore implement this library, and the _environment host_ shall inject its symbols into all `VBA` projects; the symbols shall carry the appropriate _return type_ metadata.

The SDK defines all the interfaces for the _internal representation_ of each module - the _environment host_ exposes the symbols provided by the library to the _workspace_:

- **MS-VBAL§6.1.1 Predefined Enums**
  - [FormShowConstants](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.VBFormShowConstants.html)
  - [VbAppWinStyle](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.VBAppWinStyle.html)
  - [VbCalendar](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.VBCalendar.html)
  - [VbCallType](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.VBCallType.html)
  - [VbCompareMethod](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.VBCompareMethod.html)
  - [VbDateTimeFormat](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.VBDateTimeFormat.html)
  - [VbDayOfWeek](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.VBDayOfWeek.html)
  - [VbFileAttribute](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.VBFileAttribute.html)
  - [VbFirstWeekOfYear](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.VBFirstWeekOfYear.html)
  - [VbIMEStatus](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.VBIMEStatus.html)
  - [VbMsgBoxResult](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.VBMsgBoxResult.html)
  - [VbMsgBoxStyle](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.VBMsgBoxStyle.html)
  - [VbQueryClose](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.VBQueryClose.html)
  - [VbStrConv](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.VBStrConv.html)
  - [VbTriState](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.VBTriState.html)
  - [VbVarType](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.VBVarType.html)

- **MS-VBAL§6.1.2 Predefined Procedural Modules**
  - [ColorConstantsModule](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.IStdColorConstantsModule.html)
  - [ConstantsModule](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.IStdConstantsModule.html)
  - [ConversionModule](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.IStdConversionModule.html)
  - [DateTimeModule](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.IStdDateTimeModule.html)
  - [FileSystemModule](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.IStdFileSystemModule.html)
  - [FinancialModule](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.IStdFinancialModule.html)
  - [InformationModule](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.IStdInformationModule.html)
  - [InteractionModule](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.IStdInteractionModule.html)
  - [KeyCodeConstants](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.VBKeyCodeConstants.html)
  - [MathModule](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.IStdMathModule.html)
  - [StringsModule](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.IStdStringsModule.html)
  - [SystemColorsConstants](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.VBSystemColorConstants.html)

- **MS-VBAL§6.1.3 Predefined Class Modules**
  - [CollectionClass](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.IStdCollectionClass.html)
  - [ErrClass](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.IStdErrClass.html)
  - [GlobalClass](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.IStdGlobalClass.html)

> ℹ️ The **VBScript RegExp 5.5** _regular expressions_ library was recently folded (as-is) into the **MS-VBA** _VBA Standard Library_; this reference MS-VBAL section does not actually exist, the folded VBScript library does not appear to be officially documented by its publisher at this time.

- **MS-VBAL§6.2.1 VBScript RegExp 5.5 Class Modules**
  - [RegExpClass](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.IStdRegExpClass.html)
  - [MatchClass](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.IStdMatchClass.html)
  - [MatchCollectionClass](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.IStdMatchCollectionClass.html)
  - [SubMatchesClass](../_site/api/RDCore.SDK.Runtime.Abstract.StdLib.IStdSubMatchesClass.html)

---
 V I V A T 🩷 C U C U M I S ™  

---

<p align="center">
<img alt="Logo™ 9562-7303 Québec inc." src="../images/vector-ducky.svg" style="width:200px; margin-top:72px;" /><br/>
<small>© Copyright <strong>9562-7303 Québec inc.</strong> (2026)<br/></small>
</p>

