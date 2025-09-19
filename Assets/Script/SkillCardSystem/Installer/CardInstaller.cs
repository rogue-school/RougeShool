using UnityEngine;
using Zenject;
using Game.SkillCardSystem.Factory;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Service;

namespace Game.SkillCardSystem.Installation
{
    /// <summary>
    /// SkillCardSystem용 Zenject Installer.
    /// 레지스트리/팩토리 등 핵심 서비스를 바인딩합니다.
    /// </summary>
    public class CardInstaller : MonoInstaller<CardInstaller>
    {
        [Header("레지스트리(씬 내 컴포넌트 참조)")]
        [SerializeField] private SkillCardRegistry skillCardRegistry;

        public override void InstallBindings()
        {
            // 레지스트리: 씬 객체 주입(없으면 경고)
            if (skillCardRegistry != null)
            {
                Container.Bind<SkillCardRegistry>().FromInstance(skillCardRegistry).AsSingle();
            }
            else
            {
                Debug.LogWarning("[CardInstaller] SkillCardRegistry가 연결되지 않았습니다. 런타임 카드 조회가 제한될 수 있습니다.");
            }

            // 팩토리
            Container.BindInterfacesTo<SkillCardFactory>().AsSingle();
        }
    }
}


