///-----------------------------------------------------------------
/// Author : Adrien Lemaire
/// Date : 26/10/2022 17:39
///-----------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace com.polywitch.arithmeticllparser {
	public class MathLLParser
	{
        private static readonly List<string> numberList = new List<string>()
        {
            "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", ".", ","
        };

        private static List<Token> tokens = new List<Token>();

        private static Token currentToken;

        private static int indexToken = 0;

        public static double EvalMathExpression(string expression)
        {
            tokens = ConvertToTokens(expression);

            if (tokens.Count == 0)
            {
                Debug.LogError("Expression is empty");
                return 0f;
            }

            indexToken = 0;
            currentToken = tokens[indexToken];

            Token result = Expression();

            if (currentToken)
            {
                Debug.LogError("Invalide Operation.");
                return 0f;
            }

            return result.value;
        }

        #region Parser
        private static Token Expression()
        {
            Token result = Term();

            TokenType currentType;

            while (currentToken && (currentToken.tokenType == TokenType.PLUS || currentToken.tokenType == TokenType.MINUS))
            {
                currentType = currentToken.tokenType;
                Advance();
                result = Operate(currentType, result, Term());
            }

            return result;
        }

        private static Token Term()
        {
            Token result = Factor();

            TokenType currentType;

            while (currentToken && (currentToken.tokenType == TokenType.MULTIPLY || currentToken.tokenType == TokenType.DIVIDE))
            {
                currentType = currentToken.tokenType;
                Advance();
                result = Operate(currentType, result, Factor());
            }

            return result;
        }

        private static Token Factor()
        {
            Token result = currentToken;

            TokenType currentType;

            if (currentToken.tokenType == TokenType.LEFT_BRACKET)
            {
                Advance();
                result = Expression();

                if (currentToken.tokenType != TokenType.RIGHT_BRACKET)
                    return RaiseError();

                Advance();
                return result;
            }
            else if (currentToken.tokenType == TokenType.NUMBER)
            {
                Advance();
                return result;
            }
            else if (currentToken.tokenType == TokenType.PLUS || currentToken.tokenType == TokenType.MINUS)
            {
                currentType = currentToken.tokenType;
                Advance();
                return Operate(currentType, Token.None, Factor());
            }
            else return RaiseError();
        }

        private static Token Operate(TokenType _operator, Token leftValue, Token rightValue)
        {
            //Debug.Log("Operate : " + leftValue.value + " " + _operator + " " + rightValue.value);

            Token returnValue = new Token();

            switch (_operator)
            {
                case TokenType.PLUS:
                    returnValue.value = leftValue.value + rightValue.value;
                    break;
                case TokenType.MINUS:
                    returnValue.value = leftValue.value - rightValue.value;
                    break;
                case TokenType.MULTIPLY:
                    returnValue.value = leftValue.value * rightValue.value;
                    break;
                case TokenType.DIVIDE:
                    returnValue.value = leftValue.value / rightValue.value;
                    break;
                default:
                    Debug.LogError($"Invalid operator : {_operator}");
                    return null;
            }

            return returnValue;
        }

        private static void Advance()
        {
            if (indexToken == tokens.Count - 1)
            {
                indexToken = 0;
                currentToken = null;
            }
            else
            {
                indexToken++;
                currentToken = tokens[indexToken];
            }
        }

        private static Token RaiseError()
        {
            Debug.LogError("Invalide Operation.");
            return null;
        }
        #endregion

        #region Lexer
        private static List<Token> ConvertToTokens(string expression)
        {
            List<Token> tokens = new List<Token>();

            string character;

            string currentNumber = string.Empty;

            for (int i = 0; i < expression.Length; i++)
            {
                character = expression[i].ToString();

                if (numberList.Contains(character))
                {
                    if (character == ".")
                        character = ",";

                    currentNumber += character;
                }
                else
                {
                    if (currentNumber != string.Empty)
                    {
                        tokens.Add(CreateNumberToken(double.Parse(currentNumber)));
                        currentNumber = string.Empty;
                    }

                    Token _token = CreateCharacterToken(character);

                    if (_token) tokens.Add(_token);
                }
            }

            if (currentNumber != string.Empty)
                tokens.Add(CreateNumberToken(double.Parse(currentNumber)));

            return tokens;
        }

        private static Token CreateCharacterToken(string character)
        {
            Token returnToken = new Token();

            switch (character)
            {
                case "+":
                    returnToken.tokenType = TokenType.PLUS;
                    break;
                case "-":
                    returnToken.tokenType = TokenType.MINUS;
                    break;
                case "*":
                    returnToken.tokenType = TokenType.MULTIPLY;
                    break;
                case "/":
                    returnToken.tokenType = TokenType.DIVIDE;
                    break;
                case "(":
                    returnToken.tokenType = TokenType.LEFT_BRACKET;
                    break;
                case ")":
                    returnToken.tokenType = TokenType.RIGHT_BRACKET;
                    break;
                case " ": return null;
                default:
                    Debug.LogError($"character {character} is not supported by this operation.");
                    return null;
            }

            return returnToken;
        }

        private static Token CreateNumberToken(double value)
            => new Token() { value = value };
        #endregion

        private enum TokenType
        {
            NUMBER,
            MULTIPLY,
            DIVIDE,
            PLUS,
            MINUS,
            LEFT_BRACKET,
            RIGHT_BRACKET
        }

        private class Token
        {
            public TokenType tokenType { get; set; } = TokenType.NUMBER;
            public double value { get; set; } = 0;


            public static implicit operator bool(Token value)
                => value != null;

            public static Token None
                => new Token();
        }
    }
}
