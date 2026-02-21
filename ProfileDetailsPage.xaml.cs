using LocalLink.Repositories;

namespace LocalLink.Vistas.Profile
{
    public partial class ProfileDetailsPage : ContentPage
    {
        private readonly UserRepository _userRepository;
        private readonly string _email;

        public ProfileDetailsPage(UserRepository userRepository, string email)
        {
            InitializeComponent();

            _userRepository = userRepository;
            _email = email;

            LoadUserData();
        }

        private async void LoadUserData()
        {
            if (string.IsNullOrEmpty(_email))
            {
                await DisplayAlert("Error", "No se recibió el email del perfil.", "OK");
                await Navigation.PopAsync();
                return;
            }

            var users = await _userRepository.GetAllUsersAsync();

            var user = users.FirstOrDefault(u =>
                u.Email.Equals(_email, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                await DisplayAlert("Error", "Usuario no encontrado.", "OK");
                await Navigation.PopAsync();
                return;
            }

            UsernameLabel.Text = user.Username?.ToUpper();
            EmailLabel.Text = user.Email;

            ProfileImage.Source = string.IsNullOrEmpty(user.ProfileImagePath)
                ? "default_avatar.png"
                : user.ProfileImagePath;
        }

        private async void OnEditProfileClicked(object sender, EventArgs e)
        {
            EditUsernameEntry.Text = UsernameLabel.Text;
            EditProfileImage.Source = ProfileImage.Source;

            Shell.SetTabBarIsVisible(this, false);
            EditOverlay.IsVisible = true;
            // Esperar que se mida el contenido
            await Task.Delay(10);

            EditModal.TranslationY = EditModal.Height;

            await EditModal.TranslateTo(0, 0, 250, Easing.CubicOut);
            
        }

        private async Task CloseEditModalAsync()
        {
            await EditModal.TranslateTo(0, EditModal.Height, 200, Easing.CubicIn);
            EditOverlay.IsVisible = false;
            Shell.SetTabBarIsVisible(this, true);
        }

        private async void OnCancelEditClicked(object sender, EventArgs e)
        {
            await CloseEditModalAsync();
        }

        private async void OnSaveEditClicked(object sender, EventArgs e)
        {
            string newUsername = EditUsernameEntry.Text?.Trim();

            if (string.IsNullOrEmpty(newUsername))
            {
                await DisplayAlert("Error", "El nombre no puede estar vacío.", "OK");
                return;
            }

            var users = await _userRepository.GetAllUsersAsync();

            var user = users.FirstOrDefault(u =>
                u.Email.Equals(_email, StringComparison.OrdinalIgnoreCase));

            if (user == null)
                return;

            user.Username = newUsername;

            await _userRepository.UpdateUserAsync(user);

            UsernameLabel.Text = newUsername.ToUpper();

            await CloseEditModalAsync();
        }

        private async void OnChangePhotoClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Foto", "Aquí puedes abrir el selector de imágenes.", "OK");
        }
    }
}

