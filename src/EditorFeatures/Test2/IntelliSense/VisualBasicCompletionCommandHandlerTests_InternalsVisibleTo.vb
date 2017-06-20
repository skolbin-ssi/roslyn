﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Threading.Tasks

Namespace Microsoft.CodeAnalysis.Editor.UnitTests.IntelliSense

    Public Class VisualBasicCompletionCommandHandlerTests_InternalsVisibleTo

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function CodeCompletionContainsOtherAssembliesOfSolution() As Task
            Using state = TestState.CreateTestStateFromWorkspace(
                <Workspace>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="ClassLibrary1"/>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="ClassLibrary2"/>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="ClassLibrary3"/>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="TestAssembly">
                        <Document FilePath="A.vb"><![CDATA[
<Assembly: System.Runtime.CompilerServices.InternalsVisibleTo("$$
]]>
                        </Document>
                    </Project>
                </Workspace>)
                state.SendInvokeCompletionList()
                Await state.AssertCompletionSession()
                Assert.True(state.CompletionItemsContainsAll({"ClassLibrary1", "ClassLibrary2", "ClassLibrary3"}))
            End Using
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function CodeCompletionContainsOtherAssemblyIfAttributeSuffixIsPresent() As Task
            Using state = TestState.CreateTestStateFromWorkspace(
                <Workspace>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="ClassLibrary1"/>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="TestAssembly">
                        <Document FilePath="A.vb"><![CDATA[
<Assembly: System.Runtime.CompilerServices.InternalsVisibleToAttribute("$$
]]>
                        </Document>
                    </Project>
                </Workspace>)
                state.SendInvokeCompletionList()
                Await state.AssertCompletionSession()
                Assert.True(state.CompletionItemsContainsAll({"ClassLibrary1"}))
            End Using
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function CodeCompletionIsTriggeredWhenDoubleQuoteIsEntered() As Task
            Using state = TestState.CreateTestStateFromWorkspace(
                <Workspace>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="ClassLibrary1"/>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="TestAssembly">
                        <Document FilePath="A.vb"><![CDATA[
<Assembly: System.Runtime.CompilerServices.InternalsVisibleTo($$
]]>
                        </Document>
                    </Project>
                </Workspace>)
                Await state.AssertNoCompletionSession()
                state.SendTypeChars(""""c)
                Await state.AssertCompletionSession()
                Assert.True(state.CompletionItemsContainsAll({"ClassLibrary1"}))
            End Using
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function CodeCompletionContainsIsEmptyUntilDoubleQuotesAreEntered() As Task
            Using state = TestState.CreateTestStateFromWorkspace(
                <Workspace>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="ClassLibrary1"/>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="TestAssembly">
                        <Document FilePath="A.vb"><![CDATA[
<Assembly: System.Runtime.CompilerServices.InternalsVisibleTo$$
]]>
                        </Document>
                    </Project>
                </Workspace>)
                Await state.AssertNoCompletionSession()
                state.SendInvokeCompletionList()
                Await state.WaitForAsynchronousOperationsAsync()
                Assert.False(state.CompletionItemsContainsAny({"ClassLibrary1"}))
                state.SendTypeChars("("c)
                Await state.WaitForAsynchronousOperationsAsync()
                state.SendInvokeCompletionList()
                Await state.WaitForAsynchronousOperationsAsync()
                Assert.False(state.CompletionItemsContainsAny({"ClassLibrary1"}))
                state.SendTypeChars(""""c)
                Await state.AssertCompletionSession()
                Assert.True(state.CompletionItemsContainsAll({"ClassLibrary1"}))
            End Using
        End Function

        Private Async Function AssertCompletionListHasItems(code As String, hasItems As Boolean) As Task
            Using state = TestState.CreateTestStateFromWorkspace(
                <Workspace>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="ClassLibrary1"/>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="TestAssembly">
                        <Document FilePath="A.vb">
Imports System.Runtime.CompilerServices
Imports System.Reflection
<%= code %>
                        </Document>
                    </Project>
                </Workspace>)
                state.SendInvokeCompletionList()
                Await state.WaitForAsynchronousOperationsAsync()
                If hasItems Then
                    Await state.AssertCompletionSession()
                    Assert.True(state.CompletionItemsContainsAll({"ClassLibrary1"}))
                Else
                    If Not state.CurrentCompletionPresenterSession Is Nothing Then
                        Assert.False(state.CompletionItemsContainsAny({"ClassLibrary1"}))
                    End If
                End If
            End Using
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function AssertCompletionListHasItems_AfterSingleDoubleQuoteAndClosing() As Task
            Await AssertCompletionListHasItems("<Assembly: InternalsVisibleTo(""$$)>", True)
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function AssertCompletionListHasItems_AfterText() As Task
            Await AssertCompletionListHasItems("<Assembly: InternalsVisibleTo(""Test$$)>", True)
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function AssertCompletionListHasItems_IfCursorIsInSecondParameter() As Task
            Await AssertCompletionListHasItems("<Assembly: InternalsVisibleTo(""Test"", ""$$", True)
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function AssertCompletionListHasNoItems_IfCursorIsClosingDoubleQuote1() As Task
            Await AssertCompletionListHasItems("<Assembly: InternalsVisibleTo(""Test""$$", False)
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function AssertCompletionListHasNoItems_IfCursorIsClosingDoubleQuote2() As Task
            Await AssertCompletionListHasItems("<Assembly: InternalsVisibleTo(""""$$", False)
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function AssertCompletionListHasItems_IfNamedParameterIsPresent() As Task
            Await AssertCompletionListHasItems("<Assembly: InternalsVisibleTo(""$$, AllInternalsVisible:=True)>", True)
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function AssertCompletionListHasNoItems_IfNotInternalsVisibleToAttribute() As Task
            Await AssertCompletionListHasItems("<Assembly: AssemblyVersion(""$$"")>", False)
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function AssertCompletionListHasItems_IfOtherAttributeIsPresent1() As Task
            Await AssertCompletionListHasItems("<Assembly: AssemblyVersion(""1.0.0.0""), InternalsVisibleTo(""$$", True)
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function AssertCompletionListHasItems_IfOtherAttributeIsPresent2() As Task
            Await AssertCompletionListHasItems("<Assembly: InternalsVisibleTo(""$$""), AssemblyVersion(""1.0.0.0"")>", True)
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function AssertCompletionListHasItems_IfOtherAttributesAreAhead() As Task
            Await AssertCompletionListHasItems("
                <Assembly: AssemblyVersion(""1.0.0.0"")>
                <Assembly: InternalsVisibleTo(""$$", True)
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function AssertCompletionListHasItems_IfOtherAttributesAreFollowing() As Task
            Await AssertCompletionListHasItems("
            <Assembly: InternalsVisibleTo(""$$
            <Assembly: AssemblyVersion(""1.0.0.0"")>
            <Assembly: AssemblyCompany(""Test"")>", True)
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function AssertCompletionListHasItems_IfNamespaceIsFollowing() As Task
            Await AssertCompletionListHasItems("
            <Assembly: InternalsVisibleTo(""$$
            Namespace A             
                Public Class A
                End Class
            End Namespace", True)
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function CodeCompletionHasItemsIfInteralVisibleToIsReferencedByTypeAlias() As Task
            Using state = TestState.CreateTestStateFromWorkspace(
                <Workspace>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="ClassLibrary1"/>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="TestAssembly">
                        <Document FilePath="A.vb"><![CDATA[
Imports IVT = System.Runtime.CompilerServices.InternalsVisibleToAttribute
<Assembly: IVT("$$
]]>
                        </Document>
                    </Project>
                </Workspace>)
                state.SendInvokeCompletionList()
                Await state.AssertCompletionSession()
                Assert.True(state.CompletionItemsContainsAll({"ClassLibrary1"}))
            End Using
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function CodeCompletionDoesNotContainCurrentAssembly() As Task
            Using state = TestState.CreateTestStateFromWorkspace(
                <Workspace>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="ClassLibrary1"/>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="TestAssembly">
                        <Document FilePath="A.vb"><![CDATA[
<Assembly: System.Runtime.CompilerServices.InternalsVisibleTo("$$")>
]]>
                        </Document>
                    </Project>
                </Workspace>)
                state.SendInvokeCompletionList()
                Await state.AssertCompletionSession()
                Assert.False(state.CompletionItemsContainsAny({"TestAssembly"}))
            End Using
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function CodeCompletionInsertsAssemblyNameOnCommit() As Task
            Using state = TestState.CreateTestStateFromWorkspace(
                <Workspace>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="ClassLibrary1">
                    </Project>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="TestAssembly">
                        <Document><![CDATA[
<Assembly: System.Runtime.CompilerServices.InternalsVisibleTo("$$")>
]]>
                        </Document>
                    </Project>
                </Workspace>)
                state.SendInvokeCompletionList()
                Await state.AssertSelectedCompletionItem("ClassLibrary1")
                state.SendTab()
                Await state.WaitForAsynchronousOperationsAsync()
                state.AssertMatchesTextStartingAtLine(1, "<Assembly: System.Runtime.CompilerServices.InternalsVisibleTo(""ClassLibrary1"")>")
            End Using
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function CodeCompletionInsertsPublicKeyOnCommit() As Task
            Using state = TestState.CreateTestStateFromWorkspace(
                <Workspace>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="ClassLibrary1">
                        <CompilationOptions
                            CryptoKeyFile=<%= SigningTestHelpers.PublicKeyFile %>
                            StrongNameProvider=<%= GetType(SigningTestHelpers.VirtualizedStrongNameProvider).AssemblyQualifiedName %>/>
                    </Project>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="TestAssembly">
                        <Document><![CDATA[
<Assembly: System.Runtime.CompilerServices.InternalsVisibleTo("$$")>
]]>
                        </Document>
                    </Project>
                </Workspace>)
                state.SendInvokeCompletionList()
                Await state.AssertSelectedCompletionItem("ClassLibrary1")
                state.SendTab()
                Await state.WaitForAsynchronousOperationsAsync()
                state.AssertMatchesTextStartingAtLine(1, "<Assembly: System.Runtime.CompilerServices.InternalsVisibleTo(""ClassLibrary1, PublicKey=00240000048000009400000006020000002400005253413100040000010001002b986f6b5ea5717d35c72d38561f413e267029efa9b5f107b9331d83df657381325b3a67b75812f63a9436ceccb49494de8f574f8e639d4d26c0fcf8b0e9a1a196b80b6f6ed053628d10d027e032df2ed1d60835e5f47d32c9ef6da10d0366a319573362c821b5f8fa5abc5bb22241de6f666a85d82d6ba8c3090d01636bd2bb"")>")
            End Using
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function CodeCompletionContainsPublicKeyIfKeyIsSpecifiedByAttribute() As Task
            Using state = TestState.CreateTestStateFromWorkspace(
                <Workspace>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="ClassLibrary1">
                        <CompilationOptions
                            StrongNameProvider=<%= GetType(SigningTestHelpers.VirtualizedStrongNameProvider).AssemblyQualifiedName %>/>
                        <Document>
                            &lt;Assembly: System.Reflection.AssemblyKeyFile("<%= SigningTestHelpers.PublicKeyFile %>")&gt;
                        </Document>
                    </Project>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="TestAssembly">
                        <Document><![CDATA[
<Assembly: System.Runtime.CompilerServices.InternalsVisibleTo("$$")>
]]>
                        </Document>
                    </Project>
                </Workspace>)
                state.SendInvokeCompletionList()
                Await state.AssertSelectedCompletionItem("ClassLibrary1")
                state.SendTab()
                Await state.WaitForAsynchronousOperationsAsync()
                state.AssertMatchesTextStartingAtLine(1, "<Assembly: System.Runtime.CompilerServices.InternalsVisibleTo(""ClassLibrary1, PublicKey=00240000048000009400000006020000002400005253413100040000010001002b986f6b5ea5717d35c72d38561f413e267029efa9b5f107b9331d83df657381325b3a67b75812f63a9436ceccb49494de8f574f8e639d4d26c0fcf8b0e9a1a196b80b6f6ed053628d10d027e032df2ed1d60835e5f47d32c9ef6da10d0366a319573362c821b5f8fa5abc5bb22241de6f666a85d82d6ba8c3090d01636bd2bb"")>")
            End Using
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function CodeCompletionContainsPublicKeyIfDelayedSigningIsEnabled() As Task
            Using state = TestState.CreateTestStateFromWorkspace(
                <Workspace>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="ClassLibrary1">
                        <CompilationOptions
                            CryptoKeyFile=<%= SigningTestHelpers.PublicKeyFile %>
                            StrongNameProvider=<%= GetType(SigningTestHelpers.VirtualizedStrongNameProvider).AssemblyQualifiedName %>/>
                            DelaySign="True"
                        />
                    </Project>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="TestAssembly">
                        <Document><![CDATA[
<Assembly: System.Runtime.CompilerServices.InternalsVisibleTo("$$")>
]]>
                        </Document>
                    </Project>
                </Workspace>)
                state.SendInvokeCompletionList()
                Await state.AssertSelectedCompletionItem("ClassLibrary1")
                state.SendTab()
                Await state.WaitForAsynchronousOperationsAsync()
                state.AssertMatchesTextStartingAtLine(1, "<Assembly: System.Runtime.CompilerServices.InternalsVisibleTo(""ClassLibrary1, PublicKey=00240000048000009400000006020000002400005253413100040000010001002b986f6b5ea5717d35c72d38561f413e267029efa9b5f107b9331d83df657381325b3a67b75812f63a9436ceccb49494de8f574f8e639d4d26c0fcf8b0e9a1a196b80b6f6ed053628d10d027e032df2ed1d60835e5f47d32c9ef6da10d0366a319573362c821b5f8fa5abc5bb22241de6f666a85d82d6ba8c3090d01636bd2bb"")>")
            End Using
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function CodeCompletionListIsEmptyIfAttributeIsNotTheBCLAttribute() As Task
            Using state = TestState.CreateTestStateFromWorkspace(
                <Workspace>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="ClassLibrary1"/>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="TestAssembly">
                        <Document FilePath="A.vb"><![CDATA[
<Assembly: Test.InternalsVisibleTo("$$")>
Namespace Test
	<System.AttributeUsage(System.AttributeTargets.Assembly)> _
	Public NotInheritable Class InternalsVisibleToAttribute
		Inherits System.Attribute

		Public Sub New(ignore As String)
		End Sub
	End Class
End Namespace
]]>
                        </Document>
                    </Project>
                </Workspace>)
                state.SendInvokeCompletionList()
                Await state.AssertNoCompletionSession()
            End Using
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function CodeCompletionContainsOnlyAssembliesThatAreNotAlreadyIVT() As Task
            Using state = TestState.CreateTestStateFromWorkspace(
                <Workspace>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="ClassLibrary1"/>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="ClassLibrary2"/>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="ClassLibrary3"/>
                    <Project Language="Visual Basic" CommonReferences="true" AssemblyName="TestAssembly">
                        <Document FilePath="A.vb"><![CDATA[
<Assembly: System.Runtime.CompilerServices.InternalsVisibleTo("ClassLibrary1")>
<Assembly: System.Runtime.CompilerServices.InternalsVisibleTo("ClassLibrary2")>
<Assembly: System.Runtime.CompilerServices.InternalsVisibleTo("$$
]]>
                        </Document>
                    </Project>
                </Workspace>)
                state.SendInvokeCompletionList()
                Await state.AssertCompletionSession()
                Assert.False(state.CompletionItemsContainsAny({"ClassLibrary1", "ClassLibrary2"}))
                Assert.True(state.CompletionItemsContainsAll({"ClassLibrary3"}))
            End Using
        End Function
    End Class
End Namespace
