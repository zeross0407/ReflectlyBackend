namespace Reflectly.Service
{

    using Microsoft.Extensions.Hosting;
    using Reflectly.Entity;
    using Reflectly.Services;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class CleanService : BackgroundService
    {
        private Timer? _timer;
        private readonly Account_Service _Account_Service;
        private readonly TokenService _Token_Service;
        private readonly CRUD_Service<MoodCheckin> _MoodCheckin_Service;
        private readonly CRUD_Service<UserChallenge> _UserChallenge_Service;
        private readonly CRUD_Service<Photo> _Photo_Service;
        private readonly CRUD_Service<VoiceNote> _VoiceNote_Service;
        private readonly UserReflection_Service _UserReflection_Service;
        private readonly CRUD_Service<Activity> _Activity_Service;
        private readonly CRUD_Service<Feeling> _Feeling_Service;
        public CleanService(
            Account_Service _Service,
            TokenService Token_Service,
            CRUD_Service<MoodCheckin> MoodCheckin_Service,
            CRUD_Service<Photo> Photo_Service,
            CRUD_Service<VoiceNote> VoiceNote_Service,
            UserReflection_Service userReflection_Service,
            CRUD_Service<UserChallenge> userChallenge,
            CRUD_Service<Activity> Activity_Service,
            CRUD_Service<Feeling> Feeling_Service
            )
        {
            _Account_Service = _Service;
            _Token_Service = Token_Service;
            _MoodCheckin_Service = MoodCheckin_Service;
            _Photo_Service = Photo_Service;
            _VoiceNote_Service = VoiceNote_Service;
            _UserReflection_Service = userReflection_Service;
            _UserChallenge_Service = userChallenge;
            _Feeling_Service = Feeling_Service;
            _Activity_Service = Activity_Service;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {

            var now = DateTime.Now;
            var scheduledTime = DateTime.Now.AddSeconds(20);
            if (now > scheduledTime)
            {
                scheduledTime = scheduledTime.AddDays(1);
            }

            var initialDelay = scheduledTime - now;

            _timer = new Timer(DoWork, null, initialDelay, TimeSpan.FromDays(1)); // Lặp lại mỗi ngày
            return Task.CompletedTask;
        }

        private async void DoWork(object? state)
        {
            try
            {
                //for(int i=0; i<1000; i++)
                //{
                //    MoodCheckin m = new MoodCheckin{
                //        Activities = [],
                //        Feelings = [],
                //        Mood = 1.1,
                //        SubmitTime = DateTime.Now .AddHours(-i),
                //        UserId = "67567047d67f980a0e461c2e"
                //    };
                //  await  _MoodCheckin_Service.AddAsync(m);
                //}



                List<Account> ac = (await _Account_Service.GetAsync()).Where(
                    (account) => !account.active && account.deletion_scheduled_at <= DateTime.Now).ToList();
                foreach (var account in ac)
                {
                    await _Activity_Service.DeleteByUserIdAsync(account.Id);
                    await _Feeling_Service.DeleteByUserIdAsync(account.Id);
                    await _MoodCheckin_Service.DeleteByUserIdAsync(account.Id);
                    await _Photo_Service.DeleteByUserIdAsync(account.Id);
                    await _UserChallenge_Service.DeleteByUserIdAsync(account.Id);
                    await _UserReflection_Service.DeleteAsync(account.Id);
                    await _VoiceNote_Service.DeleteAsync(account.Id);
                    await _Token_Service.DeleteByUserIdAsync(account.Id);
                    await _Account_Service.RemoveAsync(account.Id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Task failed: {ex.Message}");
            }
        }


        public override void Dispose()
        {
            _timer?.Dispose();
            base.Dispose();
        }
    }

}
