﻿// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace MyScheduledTasks.Helpers;

internal static class TaskHelpers
{
    #region MainWindow Instance
    private static readonly MainWindow _mainWindow = Application.Current.MainWindow as MainWindow;
    #endregion MainWindow Instance

    #region List all tasks
    /// <summary>
    /// Gets all scheduled tasks.
    /// Creates a list for all tasks and one for tasks not in the Microsoft folder.
    /// </summary>
    /// <remarks>Some tasks may not be listed when not running as administrator.</remarks>
    internal static void GetAllTasks()
    {
        AllTasks.All_TasksCollection.Clear();
        AllTasks.Non_MS_TasksCollection.Clear();

        Stopwatch stopwatch = Stopwatch.StartNew();
        using TaskService ts = TaskService.Instance;
        foreach (Task task in ts.AllTasks)
        {
            AllTasks allTasks = new()
            {
                TaskPath = task.Path,
                TaskName = task.Name,
                TaskFolder = task.Folder.Path,
            };
            AllTasks.All_TasksCollection.Add(allTasks);
            if (!task.Folder.Path.StartsWith(@"\Microsoft"))
            {
                AllTasks.Non_MS_TasksCollection.Add(allTasks);
            }
        }
        stopwatch.Stop();
        _log.Debug($"GetAllTasks found {AllTasks.All_TasksCollection.Count}/{AllTasks.Non_MS_TasksCollection.Count} tasks took {stopwatch.Elapsed.TotalSeconds} seconds.");
    }
    #endregion List all tasks

    #region Remove tasks
    internal static void RemoveTasks(DataGrid grid)
    {
        if (grid.SelectedItems.Count == 0)
        {
            return;
        }

        if (grid.SelectedItems.Count <= 5)
        {
            for (int i = grid.SelectedItems.Count - 1; i >= 0; i--)
            {
                ScheduledTask row = grid.SelectedItems[i] as ScheduledTask;
                _ = ScheduledTask.TaskList.Remove(row);
                _log.Info($"Removed \"{row.TaskPath}\"");
                SnackbarMsg.QueueMessage($"{GetStringResource("MsgText_Removed")} {row.TaskName}", 2000);
            }
        }
        else if (grid.SelectedItems.Count > 3)
        {
            int count = grid.SelectedItems.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                ScheduledTask row = grid.SelectedItems[i] as ScheduledTask;
                ScheduledTask.TaskList.Remove(row);
                _log.Info($"Removed \"{row.TaskPath}\"");
            }
            SnackbarMsg.QueueMessage($"{GetStringResource("MsgText_Removed")} {count} {GetStringResource("MsgText_Tasks")}", 2000);
        }
    }
    #endregion Remove tasks

    #region Run tasks
    internal static void RunTask(DataGrid grid)
    {
        if (grid.SelectedItems.Count == 0)
        {
            SystemSounds.Beep.Play();
            SnackbarMsg.ClearAndQueueMessage(GetStringResource("MsgText_RunNoneSelected"), 5000);
            return;
        }

        for (int i = grid.SelectedItems.Count - 1; i >= 0; i--)
        {
            ScheduledTask row = grid.SelectedItems[i] as ScheduledTask;
            using TaskService ts = TaskService.Instance;
            Task task = ts.GetTask(row.TaskPath);

            if (task != null)
            {
                try
                {
                    _ = task.Run();
                    SnackbarMsg.ClearAndQueueMessage($"{GetStringResource("MsgText_Running")}: {task.Name}");
                    _log.Info($"Running {task.Path}");
                    System.Threading.Tasks.Task.Delay(1250).Wait();
                }
                catch (Exception ex)
                {
                    SystemSounds.Beep.Play();
                    string msg = string.Format(GetStringResource("MsgText_RunError"), task.Name);
                    SnackbarMsg.ClearAndQueueMessage($"{msg} {GetStringResource("MsgText_SeeLogFile")}", 5000);
                    _log.Error(ex, $"Error attempting to run {task.Name}");
                }
            }
        }
    }
    #endregion Run a single task

    #region Disable Tasks
    internal static void DisableTask(DataGrid grid)
    {
        if (grid.SelectedItems.Count == 0)
        {
            SystemSounds.Beep.Play();
            SnackbarMsg.ClearAndQueueMessage(GetStringResource("MsgText_DisableNoneSelected"), 5000);
            return;
        }

        for (int i = grid.SelectedItems.Count - 1; i >= 0; i--)
        {
            ScheduledTask row = grid.SelectedItems[i] as ScheduledTask;
            using TaskService ts = TaskService.Instance;
            Task task = ts.GetTask(row.TaskPath);

            if (task != null)
            {
                try
                {
                    task.Enabled = false;
                    string msg = string.Format(GetStringResource("MsgText_Disabled"), task.Name);
                    SnackbarMsg.QueueMessage(msg, 2000);
                    _log.Info($"Disabled {task.Path}");
                }
                catch (Exception ex)
                {
                    SystemSounds.Beep.Play();
                    string msg = string.Format(GetStringResource("MsgText_DisabledError"), task.Name);
                    SnackbarMsg.ClearAndQueueMessage($"{msg} {GetStringResource("MsgText_SeeLogFile")}", 5000);
                    _log.Error(ex, $"Error attempting to disable {task.Name}");
                }
            }
        }
    }
    #endregion Disable Tasks

    #region Enable Tasks
    internal static void EnableTask(DataGrid grid)
    {
        if (grid.SelectedItems.Count == 0)
        {
            SystemSounds.Beep.Play();
            SnackbarMsg.ClearAndQueueMessage(GetStringResource("MsgText_EnableNoneSelected"), 5000);
            return;
        }

        for (int i = grid.SelectedItems.Count - 1; i >= 0; i--)
        {
            ScheduledTask row = grid.SelectedItems[i] as ScheduledTask;
            using TaskService ts = TaskService.Instance;
            Task task = ts.GetTask(row.TaskPath);

            if (task != null)
            {
                try
                {
                    task.Enabled = true;
                    string msg = string.Format(GetStringResource("MsgText_Enabled"), task.Name);
                    SnackbarMsg.QueueMessage(msg, 2000);
                    _log.Info($"Enabled {task.Path}");
                }
                catch (Exception ex)
                {
                    SystemSounds.Beep.Play();
                    string msg = string.Format(GetStringResource("MsgText_EnableError"), task.Name);
                    SnackbarMsg.ClearAndQueueMessage($"{msg} {GetStringResource("MsgText_SeeLogFile")}", 5000);
                    _log.Error(ex, $"Error attempting to enable {task.Name}");
                }
            }
        }
    }
    #endregion Enable Tasks

    #region Export Tasks
    internal static void ExportTask(DataGrid grid)
    {
        if (grid.SelectedItems.Count == 0)
        {
            SystemSounds.Beep.Play();
            SnackbarMsg.ClearAndQueueMessage(GetStringResource("MsgText_ExportNoneSelected"), 5000);
            return;
        }

        for (int i = grid.SelectedItems.Count - 1; i >= 0; i--)
        {
            ScheduledTask row = grid.SelectedItems[i] as ScheduledTask;
            using TaskService ts = TaskService.Instance;
            Task task = ts.GetTask(row.TaskPath);

            if (task != null)
            {
                try
                {
                    SaveFileDialog dialog = new()
                    {
                        Title = "Export Task to XML File",
                        Filter = "XML File|*.xml",
                        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                        FileName = task.Name + ".xml"
                    };
                    bool? result = dialog.ShowDialog();
                    if (result == true)
                    {
                        task.Export(dialog.FileName);
                        string msg = string.Format(GetStringResource("MsgText_Exported"), task.Name);
                        SnackbarMsg.QueueMessage(msg, 2000);
                        SnackbarMsg.ClearAndQueueMessage($"Exported: {task.Name}");
                        _log.Info($"Exported {task.Path}");
                    }
                }
                catch (Exception ex)
                {
                    SystemSounds.Beep.Play();
                    string msg = string.Format(GetStringResource("MsgText_ExportError"), task.Name);
                    SnackbarMsg.ClearAndQueueMessage($"{msg} {GetStringResource("MsgText_SeeLogFile")}", 5000);
                    _log.Error(ex, $"Error attempting to export {task.Path}");
                }
            }
        }
    }
    #endregion

    #region Import a task
    internal static void ImportTasks()
    {
        if (TempSettings.Setting.ImportXMLFile.Contains('\"'))
        {
            TempSettings.Setting.ImportXMLFile = TempSettings.Setting.ImportXMLFile.Trim('\"');
        }

        if (!File.Exists(TempSettings.Setting.ImportXMLFile))
        {
            MDCustMsgBox mbox = new($"{GetStringResource("ImportTask_FileNotFound")}\n\n{TempSettings.Setting.ImportXMLFile}",
                GetStringResource("ImportTask_ImportErrorHeader"),
                ButtonType.Ok,
                false,
                true,
                _mainWindow,
                true);
            _ = mbox.ShowDialog();
            return;
        }

        if (string.IsNullOrEmpty(TempSettings.Setting.ImportTaskName))
        {
            MDCustMsgBox mbox = new(GetStringResource("ImportTask_ImportErrorBlank"),
                GetStringResource("ImportTask_ImportErrorHeader"),
                ButtonType.Ok,
                false,
                true,
                _mainWindow,
                true);
            _ = mbox.ShowDialog();
            return;
        }

        if (!TempSettings.Setting.ImportTaskName.StartsWith('\\'))
        {
            TempSettings.Setting.ImportTaskName = TempSettings.Setting.ImportTaskName.Insert(0, "\\");
        }

        if (!TempSettings.Setting.ImportOverwrite && CheckTaskExists(TempSettings.Setting.ImportTaskName))
        {
            MDCustMsgBox mbox = new($"{GetStringResource("ImportTask_ImportErrorExists")} \"{TempSettings.Setting.ImportTaskName}\".",
                GetStringResource("ImportTask_ImportErrorHeader"),
                ButtonType.Ok,
                false,
                true,
                _mainWindow,
                true);
            _ = mbox.ShowDialog();
            return;
        }

        try
        {
            using (TaskDefinition td = TaskService.Instance.NewTaskFromFile(TempSettings.Setting.ImportXMLFile))
            {
                if (TempSettings.Setting.ImportRunOnlyLoggedOn)
                {
                    td.Principal.LogonType = TaskLogonType.InteractiveToken;
                }
                if (TempSettings.Setting.ImportResetCreationDate)
                {
                    td.RegistrationInfo.Date = DateTime.Now;
                }

                _ = TaskService.Instance.RootFolder.RegisterTaskDefinition(TempSettings.Setting.ImportTaskName, td);
            }

            GetAllTasks();

            _log.Info($"Imported {TempSettings.Setting.ImportXMLFile} to {TempSettings.Setting.ImportTaskName}");
            SnackbarMsg.ClearAndQueueMessage($"{TempSettings.Setting.ImportXMLFile} {GetStringResource("ImportTask_ImportSuccess")}");
            MDCustMsgBox mbox = new($"{TempSettings.Setting.ImportXMLFile} {GetStringResource("ImportTask_ImportSuccess")}",
                    GetStringResource("ImportTask_ImportSuccessHeader"),
                    ButtonType.Ok,
                    false,
                    true,
                    _mainWindow,
                    false);
            _ = mbox.ShowDialog();
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"Error Importing {TempSettings.Setting.ImportXMLFile}");
            MDCustMsgBox mbox = new($"{GetStringResource("ImportTask_ImportErrorGeneral")}\n\n{ex.Message}",
                    GetStringResource("ImportTask_ImportErrorHeader"),
                    ButtonType.Ok,
                    false,
                    true,
                    _mainWindow,
                    true);
            _ = mbox.ShowDialog();
        }
    }

    internal static void ImportCaution()
    {
        MDCustMsgBox mbox = new($"{GetStringResource("ImportTask_Caution")}",
        AppInfo.AppProduct,
        ButtonType.Ok,
        false,
        true,
        _mainWindow,
        false);
        _ = mbox.ShowDialog();
    }
    #endregion Import a task

    #region Delete tasks
    internal static void DeleteTasks(DataGrid grid)
    {
        if (grid.SelectedItems.Count == 0)
        {
            SystemSounds.Beep.Play();
            SnackbarMsg.ClearAndQueueMessage(GetStringResource("MsgText_DeleteNoneSelected"), 5000);
            return;
        }

        for (int i = grid.SelectedItems.Count - 1; i >= 0; i--)
        {
            ScheduledTask task = grid.SelectedItems[i] as ScheduledTask;
            try
            {
                string msg = string.Format(GetStringResource("MsgText_Deleted"), task.TaskPath);
                SnackbarMsg.QueueMessage(msg, 2000);
                _log.Error($"Deleted: {task.TaskPath}");
            }
            catch (Exception ex)
            {
                string msg = string.Format(GetStringResource("MsgText_DeleteError"), task.TaskPath);
                SnackbarMsg.ClearAndQueueMessage($"{msg} {GetStringResource("MsgText_SeeLogFile")}", 5000);
                _log.Error(ex, $"Error attempting to delete {task.TaskPath}");
            }
        }

        DialogHost.Close("MainDialogHost");
    }
    #endregion Delete tasks

    #region Verify task exists
    public static bool CheckTaskExists(string taskPath)
    {
        TaskService ts = TaskService.Instance;
        Task task = ts.GetTask(taskPath);
        ts.Dispose();
        return task != null;
    }
    #endregion Verify task exists
}
