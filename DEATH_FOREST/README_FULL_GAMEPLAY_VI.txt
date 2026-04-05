DEATH FOREST - BAN FULL CODE GAMEPLAY (UNITY 6000.3.11f1)

Quan trọng:
1. Day la ban FULL code gameplay. KHONG can tai them asset hay fetch GitHub.
2. LevelFactory la static class, KHONG phai component. Dung Add Component de tim LevelFactory se khong thay.
3. Cach chay dung:
   - Mo project bang Unity 6000.3.11f1
   - Vao menu Death Forest > Create Empty Play Scene
   - Mo scene Assets/Scenes/DeathForest_Play.unity
   - Bam Play
   - RuntimeBootstrap se tu tao _DeathForestBootstrap va sinh toan bo map, player, UI, enemy, item.
4. Neu mo scene rong Untitled va bam Play, van chay duoc do RuntimeBootstrap tu khoi tao sau khi scene load.
5. Da sua warning font cua Unity 6: dung LegacyRuntime.ttf thay cho Arial.ttf.

Neu sau khi mo project ma khong thay menu Death Forest:
- Assets > Refresh
- neu can thi dong Unity va mo lai project

Scripts gameplay co san trong Assets/Scripts/ (GameManager, PlayerMotor, PatrolEnemy, LevelFactory, RuntimeBootstrap, BootstrapController, ...).
