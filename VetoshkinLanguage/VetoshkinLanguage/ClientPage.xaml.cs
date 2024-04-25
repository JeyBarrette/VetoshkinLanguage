using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VetoshkinLanguage
{
    /// <summary>
    /// Логика взаимодействия для ClientPage.xaml
    /// </summary>
    public partial class ClientPage : Page
    {
        int CountInPage = 10;
        int CountRecords;
        int CountPage;
        int CurrentPage = 0;

        List<Client> CurrentPageList = new List<Client>();
        List<Client> TableList;

        public ClientPage()
        {
            InitializeComponent();
            List<Client> currentClients = VetoshkinLanguageEntities.GetContext().Client.ToList();
            ClientListView.ItemsSource = currentClients;
            ClientNumCombo.SelectedIndex = 0;
            FiltrBox.SelectedIndex = 0;
            SortBox.SelectedIndex = 0;
            TBAllRecords.Text = VetoshkinLanguageEntities.GetContext().Client.Count().ToString();
            UpdateClients();
        }

        private void UpdateClients()
        {
            var currentClients = VetoshkinLanguageEntities.GetContext().Client.ToList();

            if (SortBox.SelectedIndex == 1)
            {
                currentClients = currentClients.OrderBy(p => p.LastName).ToList();
            }
            else if (SortBox.SelectedIndex == 2)
            {
                currentClients = currentClients.Where(p => p.LastVisitDate != "Нет").OrderByDescending(p => DateTime.Parse(p.LastVisitDate)).ToList();
            }
            else if (SortBox.SelectedIndex == 3)
            {
                currentClients = currentClients.OrderByDescending(p => p.VisitCount).ToList();
            }

            if (FiltrBox.SelectedIndex == 1)
            {
                currentClients = currentClients.Where(p => p.GenderCode == "ж").ToList();
            }
            else if (FiltrBox.SelectedIndex == 2)
            {
                currentClients = currentClients.Where(p => p.GenderCode == "м").ToList();
            }

            currentClients = currentClients.Where(p => p.LastName.ToLower().Contains(TxtBoxSrch.Text.ToLower()) || 
            p.FirstName.ToLower().Contains(TxtBoxSrch.Text.ToLower()) ||
            p.Patronymic.ToLower().Contains(TxtBoxSrch.Text.ToLower()) || 
            p.Email.ToLower().Contains(TxtBoxSrch.Text.ToLower()) ||
            p.Phone.Replace("+", "").Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").ToLower().Contains(TxtBoxSrch.Text.Replace("+", "").Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").ToLower())).ToList();

            TBAllRecords.Text = VetoshkinLanguageEntities.GetContext().Client.Count().ToString();
            TBCount.Text = currentClients.Count().ToString();

            if (ClientNumCombo.SelectedIndex == 0)
            {
                CountInPage = 10;
            }
            else if (ClientNumCombo.SelectedIndex == 1)
            {
                CountInPage = 50;
            }
            else if (ClientNumCombo.SelectedIndex == 2)
            {
                CountInPage = 200;
            }
            else
            {
                CountInPage = 0;
                TBAllRecords.Text = " из " + CountRecords.ToString();
            }

            ClientListView.ItemsSource = currentClients;
            TableList = currentClients;
            ChangePage(0, 0);
        }

        private void ChangePage(int direction, int? selectedPage)
        {
            CurrentPageList.Clear();
            CountRecords = TableList.Count;
            if (CountInPage != 0)
            {
                if (CountRecords % CountInPage > 0)
                {
                    CountPage = CountRecords / CountInPage + 1;
                }
                else
                {
                    CountPage = CountRecords / CountInPage;
                }

                Boolean Ifupdate = true;

                int min;
                if (selectedPage.HasValue)
                {
                    if (selectedPage >= 0 && selectedPage <= CountPage)
                    {
                        CurrentPage = (int)selectedPage;
                        min = CurrentPage * CountInPage + CountInPage < CountRecords ? CurrentPage * CountInPage + CountInPage : CountRecords;
                        for (int i = CurrentPage * CountInPage; i < min; i++)
                        {
                            CurrentPageList.Add(TableList[i]);
                        }
                    }
                }
                else
                {
                    switch (direction)
                    {
                        case 1:
                            if (CurrentPage > 0)
                            {
                                CurrentPage--;
                                min = CurrentPage * CountInPage + CountInPage < CountRecords ? CurrentPage * CountInPage + CountInPage : CountRecords;
                                for (int i = CurrentPage * CountInPage; i < min; i++)
                                {
                                    CurrentPageList.Add(TableList[i]);
                                }
                            }
                            else
                            {
                                Ifupdate = false;
                            }
                            break;
                        case 2:
                            if (CurrentPage < CountPage - 1)
                            {
                                CurrentPage++;
                                min = CurrentPage * CountInPage + CountInPage < CountRecords ? CurrentPage * CountInPage + CountInPage : CountRecords;
                                for (int i = CurrentPage * CountInPage; i < min; i++)
                                {
                                    CurrentPageList.Add(TableList[i]);
                                }
                            }
                            else
                            {
                                Ifupdate = false;
                            }
                            break;
                    }
                }
                if (Ifupdate)
                {
                    PageListBox.Items.Clear();
                    for (int i = 1; i <= CountPage; i++)
                    {
                        PageListBox.Items.Add(i);
                    }
                    PageListBox.SelectedIndex = CurrentPage;

                    //min = CurrentPage * CountInPage + CountInPage < CountRecords ? CurrentPage * CountInPage + CountInPage : CurrentPage;
                    //TBCount.Text = min.ToString();
                    TBAllRecords.Text = " из " + CountRecords.ToString();

                    ClientListView.ItemsSource = CurrentPageList;
                    ClientListView.Items.Refresh();
                }
            }
            else
            {
                PageListBox.Items.Clear();
                PageListBox.Items.Add(1);
            }
        }

        private void LeftDirButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(1, null);
        }

        private void RightDirButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(2, null);
        }

        private void PageListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ChangePage(0, Convert.ToInt32(PageListBox.SelectedItem.ToString()) - 1);
        }

        private void ClientNumCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateClients();
        }

        private void ClientDelBTN_Click(object sender, RoutedEventArgs e)
        {
            var currentClient = (sender as Button).DataContext as Client;

            if (currentClient.VisitCount == 0)
            {
                if (MessageBox.Show("Вы точно хотите выполнить удаление?", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    VetoshkinLanguageEntities.GetContext().Client.Remove(currentClient);
                    VetoshkinLanguageEntities.GetContext().SaveChanges();
                    ClientListView.ItemsSource = VetoshkinLanguageEntities.GetContext().Client.ToList();
                    UpdateClients();
                }
            }
            else
            {
                MessageBox.Show("Невозможно выполнить удаление, так как клиент посещал школу!");
            }
        }

        private void SortBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateClients();
        }

        private void FiltrBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateClients();
        }

        private void TxtBoxSrch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateClients();
        }

        private void ClientAddEditBTN_Click(object sender, RoutedEventArgs e)
        {
            new AddEditWindow((sender as Button).DataContext as Client).ShowDialog();
            UpdateClients();
        }

        private void AddClient_Click(object sender, RoutedEventArgs e)
        {
            new AddEditWindow(null).ShowDialog();
            UpdateClients();
        }
    }
}
