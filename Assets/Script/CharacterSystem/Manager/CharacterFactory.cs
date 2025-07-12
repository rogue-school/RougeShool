using UnityEngine;
using System.Collections.Generic;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.Core;

namespace Game.CharacterSystem.Manager
{
    /// <summary>
    /// 캐릭터 팩토리 (Factory Pattern + Strategy Pattern)
    /// 다양한 전략을 사용하여 캐릭터를 생성합니다.
    /// </summary>
    public class CharacterFactory : MonoBehaviour, ICharacterFactory
    {
        [Header("팩토리 설정")]
        [SerializeField] private GameObject playerCharacterPrefab;
        [SerializeField] private GameObject enemyCharacterPrefab;
        [SerializeField] private Transform characterParent;

        private Dictionary<string, ICharacterCreationStrategy> creationStrategies = new();

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeStrategies();
        }

        #endregion

        #region 전략 초기화

        /// <summary>
        /// 캐릭터 생성 전략들을 초기화합니다.
        /// </summary>
        private void InitializeStrategies()
        {
            // 기본 전략들 등록
            RegisterStrategy(new StandardCharacterCreationStrategy());
            RegisterStrategy(new BossCharacterCreationStrategy());
            RegisterStrategy(new EliteCharacterCreationStrategy());
            RegisterStrategy(new MinionCharacterCreationStrategy());
        }

        /// <summary>
        /// 새로운 생성 전략을 등록합니다.
        /// </summary>
        /// <param name="strategy">등록할 전략</param>
        public void RegisterStrategy(ICharacterCreationStrategy strategy)
        {
            if (strategy != null)
            {
                creationStrategies[strategy.GetType().Name] = strategy;
            }
        }

        #endregion

        #region ICharacterFactory 구현

        public IPlayerCharacter CreatePlayerCharacter(PlayerCharacterData data, Vector3 position)
        {
            if (data == null)
            {
                Debug.LogError("[CharacterFactory] PlayerCharacterData가 null입니다.");
                return null;
            }

            if (playerCharacterPrefab == null)
            {
                Debug.LogError("[CharacterFactory] 플레이어 캐릭터 프리팹이 설정되지 않았습니다.");
                return null;
            }

            var instance = Instantiate(playerCharacterPrefab, position, Quaternion.identity, characterParent);
            var playerCharacter = instance.GetComponent<PlayerCharacter>();

            if (playerCharacter != null)
            {
                playerCharacter.SetCharacterData(data);
                InitializeCharacterComponents(instance);
                Debug.Log($"[CharacterFactory] 플레이어 캐릭터 생성: {data.DisplayName}");
            }

            return playerCharacter;
        }

        public IEnemyCharacter CreateEnemyCharacter(EnemyCharacterData data, Vector3 position)
        {
            if (data == null)
            {
                Debug.LogError("[CharacterFactory] EnemyCharacterData가 null입니다.");
                return null;
            }

            if (enemyCharacterPrefab == null)
            {
                Debug.LogError("[CharacterFactory] 적 캐릭터 프리팹이 설정되지 않았습니다.");
                return null;
            }

            var instance = Instantiate(enemyCharacterPrefab, position, Quaternion.identity, characterParent);
            var enemyCharacter = instance.GetComponent<EnemyCharacter>();

            if (enemyCharacter != null)
            {
                enemyCharacter.SetCharacterData(data);
                InitializeCharacterComponents(instance);
                Debug.Log($"[CharacterFactory] 적 캐릭터 생성: {data.DisplayName}");
            }

            return enemyCharacter;
        }

        public ICharacter CreateCharacter(ICharacterData data, Vector3 position, ICharacterCreationStrategy strategy)
        {
            if (data == null || strategy == null)
            {
                Debug.LogError("[CharacterFactory] 데이터 또는 전략이 null입니다.");
                return null;
            }

            var character = strategy.CreateCharacter(data, position);
            if (character != null)
            {
                InitializeCharacterComponents(character.Transform.gameObject);
                Debug.Log($"[CharacterFactory] 전략을 사용한 캐릭터 생성: {data.DisplayName}");
            }

            return character;
        }

        #endregion

        #region 내부 메서드

        /// <summary>
        /// 캐릭터 컴포넌트들을 초기화합니다.
        /// </summary>
        /// <param name="characterObject">초기화할 캐릭터 오브젝트</param>
        private void InitializeCharacterComponents(GameObject characterObject)
        {
            // 상태 관리자 추가
            if (characterObject.GetComponent<CharacterStateManager>() == null)
            {
                characterObject.AddComponent<CharacterStateManager>();
            }

            // 명령 관리자 추가
            if (characterObject.GetComponent<CharacterCommandManager>() == null)
            {
                characterObject.AddComponent<CharacterCommandManager>();
            }

            // 관찰자 패턴 컴포넌트 추가
            if (characterObject.GetComponent<CharacterSubject>() == null)
            {
                characterObject.AddComponent<CharacterSubject>();
            }
        }

        #endregion

        #region 편의 메서드

        /// <summary>
        /// 전략 이름으로 캐릭터를 생성합니다.
        /// </summary>
        /// <param name="data">캐릭터 데이터</param>
        /// <param name="position">생성 위치</param>
        /// <param name="strategyName">전략 이름</param>
        /// <returns>생성된 캐릭터</returns>
        public ICharacter CreateCharacterWithStrategy(ICharacterData data, Vector3 position, string strategyName)
        {
            if (creationStrategies.TryGetValue(strategyName, out ICharacterCreationStrategy strategy))
            {
                return CreateCharacter(data, position, strategy);
            }
            else
            {
                Debug.LogWarning($"[CharacterFactory] 알 수 없는 전략: {strategyName}");
                return null;
            }
        }

        /// <summary>
        /// 사용 가능한 전략 목록을 반환합니다.
        /// </summary>
        /// <returns>전략 이름 배열</returns>
        public string[] GetAvailableStrategies()
        {
            string[] strategies = new string[creationStrategies.Count];
            creationStrategies.Keys.CopyTo(strategies, 0);
            return strategies;
        }

        #endregion
    }

    #region 구체적인 생성 전략들

    /// <summary>
    /// 표준 캐릭터 생성 전략
    /// </summary>
    public class StandardCharacterCreationStrategy : ICharacterCreationStrategy
    {
        public ICharacter CreateCharacter(ICharacterData data, Vector3 position)
        {
            // 표준 캐릭터 생성 로직
            var characterObject = new GameObject($"Standard_{data.DisplayName}");
            characterObject.transform.position = position;

            // 기본 컴포넌트 추가
            var character = characterObject.AddComponent<PlayerCharacter>();
            // 데이터 설정 로직은 별도로 구현 필요

            return character;
        }
    }

    /// <summary>
    /// 보스 캐릭터 생성 전략
    /// </summary>
    public class BossCharacterCreationStrategy : ICharacterCreationStrategy
    {
        public ICharacter CreateCharacter(ICharacterData data, Vector3 position)
        {
            // 보스 캐릭터 생성 로직
            var characterObject = new GameObject($"Boss_{data.DisplayName}");
            characterObject.transform.position = position;

            // 보스 전용 컴포넌트 추가
            var character = characterObject.AddComponent<EnemyCharacter>();
            // 보스 특별 설정 로직

            return character;
        }
    }

    /// <summary>
    /// 엘리트 캐릭터 생성 전략
    /// </summary>
    public class EliteCharacterCreationStrategy : ICharacterCreationStrategy
    {
        public ICharacter CreateCharacter(ICharacterData data, Vector3 position)
        {
            // 엘리트 캐릭터 생성 로직
            var characterObject = new GameObject($"Elite_{data.DisplayName}");
            characterObject.transform.position = position;

            // 엘리트 전용 컴포넌트 추가
            var character = characterObject.AddComponent<EnemyCharacter>();
            // 엘리트 특별 설정 로직

            return character;
        }
    }

    /// <summary>
    /// 미니언 캐릭터 생성 전략
    /// </summary>
    public class MinionCharacterCreationStrategy : ICharacterCreationStrategy
    {
        public ICharacter CreateCharacter(ICharacterData data, Vector3 position)
        {
            // 미니언 캐릭터 생성 로직
            var characterObject = new GameObject($"Minion_{data.DisplayName}");
            characterObject.transform.position = position;

            // 미니언 전용 컴포넌트 추가
            var character = characterObject.AddComponent<EnemyCharacter>();
            // 미니언 특별 설정 로직

            return character;
        }
    }

    #endregion
} 