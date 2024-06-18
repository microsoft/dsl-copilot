﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.13.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419
namespace DslCopilot.ClassroomGrammar
{
  using System;
  using System.IO;
  using System.Text;
  using System.Diagnostics;
  using System.Collections.Generic;
  using Antlr4.Runtime;
  using Antlr4.Runtime.Atn;
  using Antlr4.Runtime.Misc;
  using Antlr4.Runtime.Tree;
  using DFA = Antlr4.Runtime.Dfa.DFA;

    [System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.13.1")]
    [System.CLSCompliant(false)]
    public partial class ClassroomParser : Parser
    {
        protected static DFA[] decisionToDFA;
        protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
        public const int
            T__0 = 1, T__1 = 2, T__2 = 3, T__3 = 4, T__4 = 5, T__5 = 6, T__6 = 7, T__7 = 8, T__8 = 9,
            T__9 = 10, T__10 = 11, T__11 = 12, T__12 = 13, T__13 = 14, T__14 = 15, T__15 = 16, T__16 = 17,
            T__17 = 18, T__18 = 19, T__19 = 20, Value = 21, Note = 22, COMMENT = 23, MULTILINE_COMMENT = 24,
            ID = 25, WS = 26;
        public const int
            RULE_program = 0, RULE_actionMainMethod = 1, RULE_block = 2, RULE_statement = 3,
            RULE_notesPrintStatement = 4, RULE_variableDeclaration = 5, RULE_dataType = 6,
            RULE_expression = 7;
        public static readonly string[] ruleNames = {
        "program", "actionMainMethod", "block", "statement", "notesPrintStatement",
        "variableDeclaration", "dataType", "expression"
    };

        private static readonly string[] _LiteralNames = {
        null, "'Classroom'", "'<'", "'>'", "'Action'", "'main'", "'('", "')'",
        "'{'", "'}'", "'Notes'", "'.'", "'take'", "';'", "'='", "'value'", "'note'",
        "'*'", "'/'", "'+'", "'-'"
    };
        private static readonly string[] _SymbolicNames = {
        null, null, null, null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null, "Value", "Note",
        "COMMENT", "MULTILINE_COMMENT", "ID", "WS"
    };
        public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

        [NotNull]
        public override IVocabulary Vocabulary
        {
            get
            {
                return DefaultVocabulary;
            }
        }

        public override string GrammarFileName { get { return "Classroom.g4"; } }

        public override string[] RuleNames { get { return ruleNames; } }

        public override int[] SerializedAtn { get { return _serializedATN; } }

        static ClassroomParser()
        {
            decisionToDFA = new DFA[_ATN.NumberOfDecisions];
            for (int i = 0; i < _ATN.NumberOfDecisions; i++)
            {
                decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
            }
        }

        public ClassroomParser(ITokenStream input) : this(input, Console.Out, Console.Error) { }

        public ClassroomParser(ITokenStream input, TextWriter output, TextWriter errorOutput)
        : base(input, output, errorOutput)
        {
            Interpreter = new ParserATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
        }

        public partial class ProgramContext : ParserRuleContext
        {
            [System.Diagnostics.DebuggerNonUserCode]
            public ActionMainMethodContext actionMainMethod()
            {
                return GetRuleContext<ActionMainMethodContext>(0);
            }
            public ProgramContext(ParserRuleContext parent, int invokingState)
                : base(parent, invokingState)
            {
            }
            public override int RuleIndex { get { return RULE_program; } }
        }

        [RuleVersion(0)]
        public ProgramContext program()
        {
            ProgramContext _localctx = new ProgramContext(Context, State);
            EnterRule(_localctx, 0, RULE_program);
            try
            {
                EnterOuterAlt(_localctx, 1);
                {
                    State = 16;
                    Match(T__0);
                    State = 17;
                    Match(T__1);
                    State = 18;
                    actionMainMethod();
                    State = 19;
                    Match(T__2);
                }
            }
            catch (RecognitionException re)
            {
                _localctx.exception = re;
                ErrorHandler.ReportError(this, re);
                ErrorHandler.Recover(this, re);
            }
            finally
            {
                ExitRule();
            }
            return _localctx;
        }

        public partial class ActionMainMethodContext : ParserRuleContext
        {
            [System.Diagnostics.DebuggerNonUserCode]
            public BlockContext block()
            {
                return GetRuleContext<BlockContext>(0);
            }
            public ActionMainMethodContext(ParserRuleContext parent, int invokingState)
                : base(parent, invokingState)
            {
            }
            public override int RuleIndex { get { return RULE_actionMainMethod; } }
        }

        [RuleVersion(0)]
        public ActionMainMethodContext actionMainMethod()
        {
            ActionMainMethodContext _localctx = new ActionMainMethodContext(Context, State);
            EnterRule(_localctx, 2, RULE_actionMainMethod);
            try
            {
                EnterOuterAlt(_localctx, 1);
                {
                    State = 21;
                    Match(T__3);
                    State = 22;
                    Match(T__4);
                    State = 23;
                    Match(T__5);
                    State = 24;
                    Match(T__6);
                    State = 25;
                    block();
                }
            }
            catch (RecognitionException re)
            {
                _localctx.exception = re;
                ErrorHandler.ReportError(this, re);
                ErrorHandler.Recover(this, re);
            }
            finally
            {
                ExitRule();
            }
            return _localctx;
        }

        public partial class BlockContext : ParserRuleContext
        {
            [System.Diagnostics.DebuggerNonUserCode]
            public StatementContext[] statement()
            {
                return GetRuleContexts<StatementContext>();
            }
            [System.Diagnostics.DebuggerNonUserCode]
            public StatementContext statement(int i)
            {
                return GetRuleContext<StatementContext>(i);
            }
            public BlockContext(ParserRuleContext parent, int invokingState)
                : base(parent, invokingState)
            {
            }
            public override int RuleIndex { get { return RULE_block; } }
        }

        [RuleVersion(0)]
        public BlockContext block()
        {
            BlockContext _localctx = new BlockContext(Context, State);
            EnterRule(_localctx, 4, RULE_block);
            int _la;
            try
            {
                EnterOuterAlt(_localctx, 1);
                {
                    State = 27;
                    Match(T__7);
                    State = 31;
                    ErrorHandler.Sync(this);
                    _la = TokenStream.LA(1);
                    while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 99328L) != 0))
                    {
                        {
                            {
                                State = 28;
                                statement();
                            }
                        }
                        State = 33;
                        ErrorHandler.Sync(this);
                        _la = TokenStream.LA(1);
                    }
                    State = 34;
                    Match(T__8);
                }
            }
            catch (RecognitionException re)
            {
                _localctx.exception = re;
                ErrorHandler.ReportError(this, re);
                ErrorHandler.Recover(this, re);
            }
            finally
            {
                ExitRule();
            }
            return _localctx;
        }

        public partial class StatementContext : ParserRuleContext
        {
            [System.Diagnostics.DebuggerNonUserCode]
            public NotesPrintStatementContext notesPrintStatement()
            {
                return GetRuleContext<NotesPrintStatementContext>(0);
            }
            [System.Diagnostics.DebuggerNonUserCode]
            public VariableDeclarationContext variableDeclaration()
            {
                return GetRuleContext<VariableDeclarationContext>(0);
            }
            public StatementContext(ParserRuleContext parent, int invokingState)
                : base(parent, invokingState)
            {
            }
            public override int RuleIndex { get { return RULE_statement; } }
        }

        [RuleVersion(0)]
        public StatementContext statement()
        {
            StatementContext _localctx = new StatementContext(Context, State);
            EnterRule(_localctx, 6, RULE_statement);
            try
            {
                State = 38;
                ErrorHandler.Sync(this);
                switch (TokenStream.LA(1))
                {
                    case T__9:
                        EnterOuterAlt(_localctx, 1);
                        {
                            State = 36;
                            notesPrintStatement();
                        }
                        break;
                    case T__14:
                    case T__15:
                        EnterOuterAlt(_localctx, 2);
                        {
                            State = 37;
                            variableDeclaration();
                        }
                        break;
                    default:
                        throw new NoViableAltException(this);
                }
            }
            catch (RecognitionException re)
            {
                _localctx.exception = re;
                ErrorHandler.ReportError(this, re);
                ErrorHandler.Recover(this, re);
            }
            finally
            {
                ExitRule();
            }
            return _localctx;
        }

        public partial class NotesPrintStatementContext : ParserRuleContext
        {
            [System.Diagnostics.DebuggerNonUserCode]
            public ExpressionContext expression()
            {
                return GetRuleContext<ExpressionContext>(0);
            }
            public NotesPrintStatementContext(ParserRuleContext parent, int invokingState)
                : base(parent, invokingState)
            {
            }
            public override int RuleIndex { get { return RULE_notesPrintStatement; } }
        }

        [RuleVersion(0)]
        public NotesPrintStatementContext notesPrintStatement()
        {
            NotesPrintStatementContext _localctx = new NotesPrintStatementContext(Context, State);
            EnterRule(_localctx, 8, RULE_notesPrintStatement);
            try
            {
                EnterOuterAlt(_localctx, 1);
                {
                    State = 40;
                    Match(T__9);
                    State = 41;
                    Match(T__10);
                    State = 42;
                    Match(T__11);
                    State = 43;
                    Match(T__7);
                    State = 44;
                    expression(0);
                    State = 45;
                    Match(T__8);
                    State = 46;
                    Match(T__12);
                }
            }
            catch (RecognitionException re)
            {
                _localctx.exception = re;
                ErrorHandler.ReportError(this, re);
                ErrorHandler.Recover(this, re);
            }
            finally
            {
                ExitRule();
            }
            return _localctx;
        }

        public partial class VariableDeclarationContext : ParserRuleContext
        {
            [System.Diagnostics.DebuggerNonUserCode]
            public DataTypeContext dataType()
            {
                return GetRuleContext<DataTypeContext>(0);
            }
            [System.Diagnostics.DebuggerNonUserCode] public ITerminalNode ID() { return GetToken(ClassroomParser.ID, 0); }
            [System.Diagnostics.DebuggerNonUserCode]
            public ExpressionContext expression()
            {
                return GetRuleContext<ExpressionContext>(0);
            }
            public VariableDeclarationContext(ParserRuleContext parent, int invokingState)
                : base(parent, invokingState)
            {
            }
            public override int RuleIndex { get { return RULE_variableDeclaration; } }
        }

        [RuleVersion(0)]
        public VariableDeclarationContext variableDeclaration()
        {
            VariableDeclarationContext _localctx = new VariableDeclarationContext(Context, State);
            EnterRule(_localctx, 10, RULE_variableDeclaration);
            try
            {
                EnterOuterAlt(_localctx, 1);
                {
                    State = 48;
                    dataType();
                    State = 49;
                    Match(ID);
                    State = 50;
                    Match(T__13);
                    State = 51;
                    expression(0);
                    State = 52;
                    Match(T__12);
                }
            }
            catch (RecognitionException re)
            {
                _localctx.exception = re;
                ErrorHandler.ReportError(this, re);
                ErrorHandler.Recover(this, re);
            }
            finally
            {
                ExitRule();
            }
            return _localctx;
        }

        public partial class DataTypeContext : ParserRuleContext
        {
            public DataTypeContext(ParserRuleContext parent, int invokingState)
                : base(parent, invokingState)
            {
            }
            public override int RuleIndex { get { return RULE_dataType; } }
        }

        [RuleVersion(0)]
        public DataTypeContext dataType()
        {
            DataTypeContext _localctx = new DataTypeContext(Context, State);
            EnterRule(_localctx, 12, RULE_dataType);
            int _la;
            try
            {
                EnterOuterAlt(_localctx, 1);
                {
                    State = 54;
                    _la = TokenStream.LA(1);
                    if (!(_la == T__14 || _la == T__15))
                    {
                        ErrorHandler.RecoverInline(this);
                    }
                    else
                    {
                        ErrorHandler.ReportMatch(this);
                        Consume();
                    }
                }
            }
            catch (RecognitionException re)
            {
                _localctx.exception = re;
                ErrorHandler.ReportError(this, re);
                ErrorHandler.Recover(this, re);
            }
            finally
            {
                ExitRule();
            }
            return _localctx;
        }

        public partial class ExpressionContext : ParserRuleContext
        {
            [System.Diagnostics.DebuggerNonUserCode] public ITerminalNode Value() { return GetToken(ClassroomParser.Value, 0); }
            [System.Diagnostics.DebuggerNonUserCode] public ITerminalNode Note() { return GetToken(ClassroomParser.Note, 0); }
            [System.Diagnostics.DebuggerNonUserCode] public ITerminalNode ID() { return GetToken(ClassroomParser.ID, 0); }
            [System.Diagnostics.DebuggerNonUserCode]
            public ExpressionContext[] expression()
            {
                return GetRuleContexts<ExpressionContext>();
            }
            [System.Diagnostics.DebuggerNonUserCode]
            public ExpressionContext expression(int i)
            {
                return GetRuleContext<ExpressionContext>(i);
            }
            public ExpressionContext(ParserRuleContext parent, int invokingState)
                : base(parent, invokingState)
            {
            }
            public override int RuleIndex { get { return RULE_expression; } }
        }

        [RuleVersion(0)]
        public ExpressionContext expression()
        {
            return expression(0);
        }

        private ExpressionContext expression(int _p)
        {
            ParserRuleContext _parentctx = Context;
            int _parentState = State;
            ExpressionContext _localctx = new ExpressionContext(Context, _parentState);
            ExpressionContext _prevctx = _localctx;
            int _startState = 14;
            EnterRecursionRule(_localctx, 14, RULE_expression, _p);
            try
            {
                int _alt;
                EnterOuterAlt(_localctx, 1);
                {
                    State = 64;
                    ErrorHandler.Sync(this);
                    switch (TokenStream.LA(1))
                    {
                        case Value:
                            {
                                State = 57;
                                Match(Value);
                            }
                            break;
                        case Note:
                            {
                                State = 58;
                                Match(Note);
                            }
                            break;
                        case ID:
                            {
                                State = 59;
                                Match(ID);
                            }
                            break;
                        case T__5:
                            {
                                State = 60;
                                Match(T__5);
                                State = 61;
                                expression(0);
                                State = 62;
                                Match(T__6);
                            }
                            break;
                        default:
                            throw new NoViableAltException(this);
                    }
                    Context.Stop = TokenStream.LT(-1);
                    State = 80;
                    ErrorHandler.Sync(this);
                    _alt = Interpreter.AdaptivePredict(TokenStream, 4, Context);
                    while (_alt != 2 && _alt != global::Antlr4.Runtime.Atn.ATN.INVALID_ALT_NUMBER)
                    {
                        if (_alt == 1)
                        {
                            if (ParseListeners != null)
                                TriggerExitRuleEvent();
                            _prevctx = _localctx;
                            {
                                State = 78;
                                ErrorHandler.Sync(this);
                                switch (Interpreter.AdaptivePredict(TokenStream, 3, Context))
                                {
                                    case 1:
                                        {
                                            _localctx = new ExpressionContext(_parentctx, _parentState);
                                            PushNewRecursionContext(_localctx, _startState, RULE_expression);
                                            State = 66;
                                            if (!(Precpred(Context, 5))) throw new FailedPredicateException(this, "Precpred(Context, 5)");
                                            State = 67;
                                            Match(T__16);
                                            State = 68;
                                            expression(6);
                                        }
                                        break;
                                    case 2:
                                        {
                                            _localctx = new ExpressionContext(_parentctx, _parentState);
                                            PushNewRecursionContext(_localctx, _startState, RULE_expression);
                                            State = 69;
                                            if (!(Precpred(Context, 4))) throw new FailedPredicateException(this, "Precpred(Context, 4)");
                                            State = 70;
                                            Match(T__17);
                                            State = 71;
                                            expression(5);
                                        }
                                        break;
                                    case 3:
                                        {
                                            _localctx = new ExpressionContext(_parentctx, _parentState);
                                            PushNewRecursionContext(_localctx, _startState, RULE_expression);
                                            State = 72;
                                            if (!(Precpred(Context, 3))) throw new FailedPredicateException(this, "Precpred(Context, 3)");
                                            State = 73;
                                            Match(T__18);
                                            State = 74;
                                            expression(4);
                                        }
                                        break;
                                    case 4:
                                        {
                                            _localctx = new ExpressionContext(_parentctx, _parentState);
                                            PushNewRecursionContext(_localctx, _startState, RULE_expression);
                                            State = 75;
                                            if (!(Precpred(Context, 2))) throw new FailedPredicateException(this, "Precpred(Context, 2)");
                                            State = 76;
                                            Match(T__19);
                                            State = 77;
                                            expression(3);
                                        }
                                        break;
                                }
                            }
                        }
                        State = 82;
                        ErrorHandler.Sync(this);
                        _alt = Interpreter.AdaptivePredict(TokenStream, 4, Context);
                    }
                }
            }
            catch (RecognitionException re)
            {
                _localctx.exception = re;
                ErrorHandler.ReportError(this, re);
                ErrorHandler.Recover(this, re);
            }
            finally
            {
                UnrollRecursionContexts(_parentctx);
            }
            return _localctx;
        }

        public override bool Sempred(RuleContext _localctx, int ruleIndex, int predIndex)
        {
            switch (ruleIndex)
            {
                case 7: return expression_sempred((ExpressionContext)_localctx, predIndex);
            }
            return true;
        }
        private bool expression_sempred(ExpressionContext _localctx, int predIndex)
        {
            switch (predIndex)
            {
                case 0: return Precpred(Context, 5);
                case 1: return Precpred(Context, 4);
                case 2: return Precpred(Context, 3);
                case 3: return Precpred(Context, 2);
            }
            return true;
        }

        private static int[] _serializedATN = {
        4,1,26,84,2,0,7,0,2,1,7,1,2,2,7,2,2,3,7,3,2,4,7,4,2,5,7,5,2,6,7,6,2,7,
        7,7,1,0,1,0,1,0,1,0,1,0,1,1,1,1,1,1,1,1,1,1,1,1,1,2,1,2,5,2,30,8,2,10,
        2,12,2,33,9,2,1,2,1,2,1,3,1,3,3,3,39,8,3,1,4,1,4,1,4,1,4,1,4,1,4,1,4,1,
        4,1,5,1,5,1,5,1,5,1,5,1,5,1,6,1,6,1,7,1,7,1,7,1,7,1,7,1,7,1,7,1,7,3,7,
        65,8,7,1,7,1,7,1,7,1,7,1,7,1,7,1,7,1,7,1,7,1,7,1,7,1,7,5,7,79,8,7,10,7,
        12,7,82,9,7,1,7,0,1,14,8,0,2,4,6,8,10,12,14,0,1,1,0,15,16,84,0,16,1,0,
        0,0,2,21,1,0,0,0,4,27,1,0,0,0,6,38,1,0,0,0,8,40,1,0,0,0,10,48,1,0,0,0,
        12,54,1,0,0,0,14,64,1,0,0,0,16,17,5,1,0,0,17,18,5,2,0,0,18,19,3,2,1,0,
        19,20,5,3,0,0,20,1,1,0,0,0,21,22,5,4,0,0,22,23,5,5,0,0,23,24,5,6,0,0,24,
        25,5,7,0,0,25,26,3,4,2,0,26,3,1,0,0,0,27,31,5,8,0,0,28,30,3,6,3,0,29,28,
        1,0,0,0,30,33,1,0,0,0,31,29,1,0,0,0,31,32,1,0,0,0,32,34,1,0,0,0,33,31,
        1,0,0,0,34,35,5,9,0,0,35,5,1,0,0,0,36,39,3,8,4,0,37,39,3,10,5,0,38,36,
        1,0,0,0,38,37,1,0,0,0,39,7,1,0,0,0,40,41,5,10,0,0,41,42,5,11,0,0,42,43,
        5,12,0,0,43,44,5,8,0,0,44,45,3,14,7,0,45,46,5,9,0,0,46,47,5,13,0,0,47,
        9,1,0,0,0,48,49,3,12,6,0,49,50,5,25,0,0,50,51,5,14,0,0,51,52,3,14,7,0,
        52,53,5,13,0,0,53,11,1,0,0,0,54,55,7,0,0,0,55,13,1,0,0,0,56,57,6,7,-1,
        0,57,65,5,21,0,0,58,65,5,22,0,0,59,65,5,25,0,0,60,61,5,6,0,0,61,62,3,14,
        7,0,62,63,5,7,0,0,63,65,1,0,0,0,64,56,1,0,0,0,64,58,1,0,0,0,64,59,1,0,
        0,0,64,60,1,0,0,0,65,80,1,0,0,0,66,67,10,5,0,0,67,68,5,17,0,0,68,79,3,
        14,7,6,69,70,10,4,0,0,70,71,5,18,0,0,71,79,3,14,7,5,72,73,10,3,0,0,73,
        74,5,19,0,0,74,79,3,14,7,4,75,76,10,2,0,0,76,77,5,20,0,0,77,79,3,14,7,
        3,78,66,1,0,0,0,78,69,1,0,0,0,78,72,1,0,0,0,78,75,1,0,0,0,79,82,1,0,0,
        0,80,78,1,0,0,0,80,81,1,0,0,0,81,15,1,0,0,0,82,80,1,0,0,0,5,31,38,64,78,
        80
    };

        public static readonly ATN _ATN =
            new ATNDeserializer().Deserialize(_serializedATN);


    }
}