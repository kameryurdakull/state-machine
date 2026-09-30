# State Machine for Unity

Unity için küçük, açık yaşam döngüsüne sahip bir finite state machine paketi. Makine `MonoBehaviour` değildir: durumları ve geçişleri yönetir; Unity callback'lerini uygulamanızdan çağırırsınız. Çekirdek Unity, UniTask, VContainer ve bir event bus paketine bağımlı değildir. Bu araçlar uygulama katmanında birlikte kullanılabilir.

## Kurulum

Unity 2021.3 veya üzeri ve Git gereklidir. Package Manager → **+** → **Add package from git URL...** alanına şunu girin:

```text
https://github.com/kameryurdakull/state-machine.git?path=/Packages/com.kameryurdakull.state-machine
```

`manifest.json` üzerinden kurulum:

```json
{
  "dependencies": {
    "com.kameryurdakull.state-machine": "https://github.com/kameryurdakull/state-machine.git?path=/Packages/com.kameryurdakull.state-machine"
  }
}
```

Bu URL, paket dosyaları GitHub'a gönderildikten sonra çalışır. Kararlı sürüm için URL'nin sonuna yayımlanmış bir etiket ekleyin: `#v1.0.0`.

## İlk kullanım

```csharp
using System.Threading;
using BlackProcess.StateMachine;
using UnityEngine;
using StateMachineCore = BlackProcess.StateMachine.StateMachine;

public sealed class PlayerStateHost : MonoBehaviour
{
    private StateMachineCore _machine;

    private void Awake() => _machine = new StateMachineCore();
    private void Start() => _machine.ChangeState(new IdleState());
    private void Update() => _machine.Tick();
    private void FixedUpdate() => _machine.FixedTick();
    private void OnDestroy() => _machine?.Dispose();

    private sealed class IdleState : BaseState
    {
        public override void Enter(CancellationToken cancellationToken)
        {
            Debug.Log("Idle başladı");
        }

        public override void FixedTick()
        {
            // Fizik işlemleri burada çalışır.
        }
    }
}
```

`IState` doğrudan uygulanabilir. `BaseState`, kullanılmayan callback'ler için boş varsayılanlar sağlar. `ChangeState` aynı örnek zaten aktifse `false` döndürür; `null` durum kabul etmez. `Stop` aktif durumdan çıkar, `Dispose` makineyi kapatır. Geçiş tamamlanırken başka bir geçiş başlatmak hata verir; geçişleri event handler içinde veya normal oyun akışında başlatın, `Enter`/`Exit` içinden başlatmayın.

## Round Flow örneği

Package Manager'da paketin **Samples** bölümünden **Round Flow** örneğini içe aktarın. `RoundFlow` bileşenini bir GameObject'e ekleyin ve `SelectHero`, `SelectTrinket`, `EnterRoom`, `FinishCombat` metotlarını UI butonlarına veya oyun olaylarına bağlayın. Akış: **Hero Selection → Trinket Selection → Map → Combat → Map**. Örnek, paylaştığınız `Round` yapısındaki durum sırasını proje bağımlılıkları olmadan gösterir.

## UniTask, VContainer ve event bus ile kullanım

Makineyi VContainer üzerinden `Register<StateMachine>(Lifetime.Scoped)` ile kaydedebilirsiniz. `RoundFlow` gibi bir host, makineyi constructor veya `[Inject]` üzerinden alıp Unity callback'lerinde sürer. Kendi event bus'ınızdan gelen tipli olaylar `ChangeState` çağırabilir. Olay aboneliklerini durumun `Enter` metodunda kurup `Exit` metodunda kaldırın.

Asenkron durum işleri için `Enter` parametresindeki token'ı UniTask'e aktarın:

```csharp
public override void Enter(CancellationToken cancellationToken)
{
    LoadEncounterAsync(cancellationToken).Forget();
}

private async UniTaskVoid LoadEncounterAsync(CancellationToken cancellationToken)
{
    try
    {
        await _encounterService.LoadAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        _eventBus.Publish(new EncounterReadyEvent());
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
        // Durumdan çıkış beklenen bir iptaldir.
    }
}
```

Bu parça uygulamanızdaki `UniTask`, servis ve event bus tiplerini kullanır; paketin zorunlu bağımlılığı değildir. Durum değişince veya makine durdurulunca token iptal edilir. DOTween işlemlerini de aynı yaşam döngüsüne bağlayın: `Exit` içinde tween'i `Kill` edin veya UniTask dönüşümünde iptal token'ı kullanın.

## Önceki uygulamadan farklar

Paylaştığınız sürümde `UpdatePhysics`, Unity `LateUpdate` içinden çağrılıyordu. Bu paket `FixedTick` çağrısını açıkça `FixedUpdate`'e bağlar. `CurrentState` dışarıdan yazılamaz; başlangıç ve geçişler `ChangeState` ile yapılır. Her durumun iptal token'ı, `Round` örneğindeki asenkron işler geçişten sonra tamamlandığında eski durumun yeni durumu etkilemesini önlemeye yardımcı olur. Bu API eski `FSM.StateMachine`/`FSM.BaseState` koduyla birebir uyumlu değildir; geçiş için `UpdateLogic → Tick`, `UpdatePhysics → FixedTick`, `Enter() → Enter(CancellationToken)` uyarlaması gerekir.

## Geliştirme

Paket `Packages/com.kameryurdakull.state-machine` altında gömülü olarak bulunur. Testleri Unity Test Runner'da çalıştırmak için proje `Packages/manifest.json` dosyasına `"testables": ["com.kameryurdakull.state-machine"]` ekleyin ve **Edit Mode** testlerini çalıştırın. Testler durum geçiş sırasını, iptali, `Tick`/`FixedTick` yönlendirmesini ve yaşam döngüsünü denetler.
