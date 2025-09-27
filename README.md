# 🎮 Unity 클래시로얄 모작 (Team Project)
🛠 개발 도구: Unity(C#), JetBrains Rider, GitHub Desktop <br/>
📆 개발 기간: 25.05.20 ~ 25.05.29 (약 1주) <br/>
👥 인원: 5명 <br/>
___
클래시로얄의 핵심 게임 흐름을 모작하며 **네트워크/전투/타워 시스템** 구현 경험을 쌓은 팀 프로젝트입니다. <br/>
저는 **전투 흐름 제어, 유닛 AI, 투사체 및 마법 스킬 로직, 타워 동작**을 담당했습니다. <br/>
Git을 통한 협업 과정에서 병합 충돌 문제를 겪었고, **씬·스크립트 분리 규칙**을 도입하여 효율적인 협업 환경을 구축했습니다.  
___
📸 **인게임 이미지**
<p align="center">
  <img src="https://github.com/KimJinSu-Git/Team3_UnityProject/blob/main/Assets/ScreenShots/Image1.PNG" width="200"/> &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;
  <img src="https://github.com/KimJinSu-Git/Team3_UnityProject/blob/main/Assets/ScreenShots/Image2.PNG" width="200"/> &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;
  <img src="https://github.com/KimJinSu-Git/Team3_UnityProject/blob/main/Assets/ScreenShots/Image3.png" width="200"/>
</p>

___
🔑 주요 구현 요소
* **타워 시스템** 👉 [TowerContoller.cs](https://github.com/KimJinSu-Git/Team3_UnityProject/blob/main/Assets/02_Scripts/Tower/TowerController.cs)
  * King/Princess Tower 프리팹 구성
  * 적 감지 시 공격 애니메이션 + 화살 발사
* **유닛 시스템**
  * NavMeshAgent를 활용한 경로 탐지 👉 [BaseMonsterController.cs](https://github.com/KimJinSu-Git/Team3_UnityProject/blob/main/Assets/02_Scripts/Monsters/BaseMonsterContorller.cs)
  * Idle, Move, Attack, Die FSM 상태 관리
* **투사체/스킬 시스템**
  * Fireball, ArrowRain 마법 구현 (범위 판정 + 이펙트)
  * 유닛/타워/마법에 공통으로 적용 가능한 `Projectile` 구조 설계
* **게임 매니저 흐름**
  * Elixir 자원 관리 및 소환 제약
  * 타이머, CrownScore UI 동기화
  * 게임 종료 시점(타워 파괴 조건)에 따라 승패 판정
* **네트워크 처리**
  * 타워 파괴/마법 소환 이벤트를 RPC로 브로드캐스트 👉 [ArrowRainSpell.cs](https://github.com/KimJinSu-Git/Team3_UnityProject/blob/main/Assets/02_Scripts/Magics/ArrowRainSpell.cs#L36)
___
* **영상 바로가기** [SkullRoyal.avi](https://drive.google.com/file/d/1J9pS02dEnYSveLgmGjIX0Cc2bIAb5LII/view?usp=drive_link)
* **문서 바로가기** [SkullRoyal.pdf](https://drive.google.com/file/d/1iamMj0SPfmjJTDc4XA81wQjZerIuj447/view?usp=drive_link)
