using System;
using System.Text.RegularExpressions;

namespace Game.CoreSystem.Utility
{
    /// <summary>
    /// 한글 텍스트 처리 유틸리티 클래스
    /// 한글 조사, 숫자 변환, 텍스트 포맷팅 등을 제공합니다.
    /// </summary>
    public static class KoreanTextHelper
    {
        #region 한글 조사 처리

        /// <summary>
        /// 숫자에 적절한 한글 조사를 붙입니다.
        /// </summary>
        /// <param name="number">숫자</param>
        /// <param name="particle">조사 ("을/를", "이/가", "은/는" 등)</param>
        /// <returns>조사가 붙은 문자열</returns>
        public static string AddKoreanParticle(int number, string particle)
        {
            if (string.IsNullOrEmpty(particle))
                return number.ToString();

            // 숫자를 한글 문자열로 변환
            string numberStr = number.ToString();
            char lastDigit = numberStr[numberStr.Length - 1];

            // 마지막 자릿수에 따라 조사 결정
            bool useFirstParticle = IsFirstParticle(lastDigit);

            if (particle.Contains("/"))
            {
                string[] particles = particle.Split('/');
                return $"{number}{(useFirstParticle ? particles[0] : particles[1])}";
            }

            return $"{number}{particle}";
        }

        /// <summary>
        /// 첫 번째 조사를 사용할지 결정합니다.
        /// </summary>
        /// <param name="lastDigit">마지막 자릿수</param>
        /// <returns>첫 번째 조사 사용 여부</returns>
        private static bool IsFirstParticle(char lastDigit)
        {
            // 받침이 있는 경우 첫 번째 조사 사용
            // 1, 3, 6, 7, 8, 0은 받침이 있음
            return lastDigit == '1' || lastDigit == '3' || lastDigit == '6' || 
                   lastDigit == '7' || lastDigit == '8' || lastDigit == '0';
        }

        #endregion

        #region 숫자 변환

        /// <summary>
        /// 숫자를 한글로 변환합니다.
        /// </summary>
        /// <param name="number">변환할 숫자</param>
        /// <returns>한글 숫자 문자열</returns>
        public static string NumberToKorean(int number)
        {
            if (number < 0) return "마이너스 " + NumberToKorean(-number);
            if (number == 0) return "영";

            string[] units = { "", "일", "이", "삼", "사", "오", "육", "칠", "팔", "구" };
            string[] tens = { "", "십", "백", "천" };
            string[] bigUnits = { "", "만", "억", "조", "경" };

            if (number < 10) return units[number];
            if (number < 100) return ConvertTens(number, units, tens);
            if (number < 10000) return ConvertHundreds(number, units, tens);
            
            return ConvertBigNumbers(number, units, tens, bigUnits);
        }

        private static string ConvertTens(int number, string[] units, string[] tens)
        {
            int ten = number / 10;
            int one = number % 10;
            
            string result = "";
            if (ten > 1) result += units[ten];
            result += tens[1];
            if (one > 0) result += units[one];
            
            return result;
        }

        private static string ConvertHundreds(int number, string[] units, string[] tens)
        {
            int hundred = number / 100;
            int remainder = number % 100;
            
            string result = "";
            if (hundred > 1) result += units[hundred];
            result += tens[2];
            
            if (remainder > 0)
            {
                if (remainder < 10)
                    result += units[remainder];
                else
                    result += ConvertTens(remainder, units, tens);
            }
            
            return result;
        }

        private static string ConvertBigNumbers(int number, string[] units, string[] tens, string[] bigUnits)
        {
            // 간단한 구현 (만 단위까지만)
            if (number < 100000000) // 억 미만
            {
                int man = number / 10000;
                int remainder = number % 10000;
                
                string result = "";
                if (man > 1) result += NumberToKorean(man);
                result += bigUnits[1];
                
                if (remainder > 0)
                {
                    if (remainder < 100)
                        result += NumberToKorean(remainder);
                    else
                        result += ConvertHundreds(remainder, units, tens);
                }
                
                return result;
            }
            
            return number.ToString(); // 복잡한 경우는 숫자 그대로 반환
        }

        #endregion

        #region 텍스트 포맷팅

        /// <summary>
        /// 텍스트에서 숫자에 조사를 자동으로 붙입니다.
        /// </summary>
        /// <param name="text">원본 텍스트</param>
        /// <param name="particle">조사 ("을/를", "이/가", "은/는" 등)</param>
        /// <returns>조사가 붙은 텍스트</returns>
        public static string FormatNumbersWithParticle(string text, string particle)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(particle))
                return text;

            // 숫자 패턴 찾기 (정수만)
            string pattern = @"\b(\d+)\b";
            
            return Regex.Replace(text, pattern, match =>
            {
                int number = int.Parse(match.Value);
                return AddKoreanParticle(number, particle);
            });
        }

        /// <summary>
        /// 피해량 텍스트를 포맷팅합니다.
        /// "피해 N을 줍니다" 형태로 변환
        /// </summary>
        /// <param name="damage">피해량</param>
        /// <returns>포맷팅된 피해 텍스트</returns>
        public static string FormatDamageText(int damage)
        {
            return $"피해 {AddKoreanParticle(damage, "을/를")} 줍니다";
        }

        /// <summary>
        /// 턴 수 텍스트를 포맷팅합니다.
        /// "N턴 동안" 형태로 변환
        /// </summary>
        /// <param name="turns">턴 수</param>
        /// <returns>포맷팅된 턴 텍스트</returns>
        public static string FormatTurnText(int turns)
        {
            return $"{turns}턴 동안";
        }

        /// <summary>
        /// 횟수 텍스트를 포맷팅합니다.
        /// "N번" 형태로 변환
        /// </summary>
        /// <param name="count">횟수</param>
        /// <returns>포맷팅된 횟수 텍스트</returns>
        public static string FormatCountText(int count)
        {
            return $"{AddKoreanParticle(count, "을/를")}번";
        }

        #endregion

        #region 특수 케이스 처리

        /// <summary>
        /// 스킬카드 툴팁용 피해 텍스트를 생성합니다.
        /// </summary>
        /// <param name="damage">피해량</param>
        /// <param name="hits">히트 수 (기본값: 1)</param>
        /// <returns>포맷팅된 피해 텍스트</returns>
        public static string FormatSkillCardDamageText(int damage, int hits = 1)
        {
            string damageText = FormatDamageText(damage);
            
            if (hits > 1)
            {
                string hitText = FormatCountText(hits);
                return damageText.Replace("피해", $"피해 {hitText}");
            }
            
            return damageText;
        }

        /// <summary>
        /// 출혈 효과 텍스트를 생성합니다.
        /// </summary>
        /// <param name="damage">피해량</param>
        /// <param name="turns">지속 턴</param>
        /// <param name="effectName">이펙트 이름 (기본값: "출혈")</param>
        /// <returns>포맷팅된 출혈 텍스트</returns>
        public static string FormatBleedEffectText(int damage, int turns, string effectName = "출혈")
        {
            string turnText = FormatTurnText(turns);
            
            return $"{damage} 피해를 {turnText} 입히는 {effectName}을 부여합니다";
        }

        #endregion
    }
}
