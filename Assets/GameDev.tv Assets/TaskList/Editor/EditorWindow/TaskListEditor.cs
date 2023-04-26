using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


namespace GameDevTV.Tasks
{
    public class TaskListEditor : EditorWindow
    {
        VisualElement container;
        ObjectField savedTasksObjectField;
        Button loadTasksButton;
        TextField taskText;
        Button addTaskButton;
        ScrollView taskListScrollView;
        Button saveProgressButton;
        ProgressBar taskProgressBar;
        ToolbarSearchField searchBox;
        Label notificationLabel;

        TaskListSO taskListSO;

        public const string path = "Assets/GameDev.tv Assets/TaskList/Editor/EditorWindow/";

        [MenuItem("GameDev.tv/Task List")]
        public static void ShowWindow()
        {
            TaskListEditor window = GetWindow<TaskListEditor>();
            window.titleContent = new GUIContent("Task List");
        }

        public void CreateGUI()
        {
            container = rootVisualElement;
            VisualTreeAsset original = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path + "TaskListEditor.uxml");
            container.Add(original.Instantiate());

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path + "TaskListEditor.uss");
            container.styleSheets.Add(styleSheet);

            savedTasksObjectField = container.Q<ObjectField>("savedTasksObjectField");
            savedTasksObjectField.objectType = typeof(TaskListSO);

            loadTasksButton = container.Q<Button>("loadTasksButton");
            loadTasksButton.clicked += LoadTasks;

            taskText = container.Q<TextField>("taskText");
            taskText.RegisterCallback<KeyDownEvent>(AddTask);

            addTaskButton = container.Q<Button>("addTaskButton");
            addTaskButton.clicked += AddTask;

            taskListScrollView = container.Q<ScrollView>("taskList");

            saveProgressButton = container.Q<Button>("saveProgressButton");
            saveProgressButton.clicked += SaveProgress;

            taskProgressBar = container.Q<ProgressBar>("taskProgressBar");

            searchBox = container.Q<ToolbarSearchField>("searchBox");
            searchBox.RegisterValueChangedCallback(OnSearchTextChanged);

            notificationLabel = container.Q<Label>("notificationLabel");
            UpdateNotifications("Please load a task list to continue.");

        }

        void AddTask()
        {
            if (!string.IsNullOrEmpty(taskText.value))
            {
                taskListScrollView.Add(CreateTask(taskText.value));
                SaveTask(taskText.value);
                taskText.value = "";
                taskText.Focus();
                UpdateProgress();
                UpdateNotifications("Task added successfully");
            }

        }

        TaskItem CreateTask(string taskText)
        {
            TaskItem taskItem = new TaskItem(taskText);
            taskItem.GetTaskLabel().text = taskText;
            taskItem.GetTaskToggle().RegisterValueChangedCallback(UpdateProgress);
            return taskItem;
        }

        void AddTask(KeyDownEvent e)
        {
            if (Event.current.Equals(Event.KeyboardEvent("Return")))
            {
                AddTask();
            }
        }

        void LoadTasks()
        {
            taskListSO = savedTasksObjectField.value as TaskListSO;

            if (taskListSO != null)
            {
                taskListScrollView.Clear();
                List<string> tasks = taskListSO.GetTasks();

                foreach (string task in tasks)
                {
                    taskListScrollView.Add(CreateTask(task));
                }

                UpdateProgress();
                UpdateNotifications(taskListSO.name + " successfully loaded.");
            }
            else
            {
                UpdateNotifications("Failed to load task list.");
            }
        }

        void SaveTask(string task)
        {
            taskListSO.AddTask(task);
            EditorUtility.SetDirty(taskListSO);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UpdateNotifications("Task added successfully.");
        }

        void SaveProgress()
        {
            if (taskListSO != null)
            {
                List<string> tasks = new List<string>();

                foreach (TaskItem task in taskListScrollView.Children())
                {
                    if (!task.GetTaskToggle().value)
                    {
                        tasks.Add(task.GetTaskLabel().text);
                    }
                }

                taskListSO.AddTasks(tasks);
                EditorUtility.SetDirty(taskListSO);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                LoadTasks();
                UpdateNotifications("Tasks saved successfully.");
            }
        }

        void UpdateProgress()
        {
            int count = 0;
            int completed = 0;

            foreach (TaskItem task in taskListScrollView.Children())
            {
                if (task.GetTaskToggle().value)
                {
                    completed++;
                }
                count++;
            }

            if (count > 0)
            {
                float progress = completed / (float)count;
                taskProgressBar.value = progress;
                taskProgressBar.title = string.Format("{0} %", Mathf.Round(progress * 1000) / 10f);
                UpdateNotifications("Progress updated. Don't forget to save!");
            }
            else
            {
                taskProgressBar.value = 1;
                taskProgressBar.title = string.Format("{0} %", 100);
            }
        }

        void UpdateProgress(ChangeEvent<bool> e)
        {
            UpdateProgress();
        }

        void OnSearchTextChanged(ChangeEvent<string> changeEvent)
        {
            string searchText = changeEvent.newValue.ToUpper();

            foreach(TaskItem task in taskListScrollView.Children())
            {
                string taskText = task.GetTaskLabel().text.ToUpper();

                if(!string.IsNullOrEmpty(searchText) && taskText.Contains(searchText))
                {
                    task.GetTaskLabel().AddToClassList("highlight");
                }
                else
                {
                    task.GetTaskLabel().RemoveFromClassList("highlight");
                }
            }
        }

        void UpdateNotifications(string text)
        {
            if(!string.IsNullOrEmpty(text))
            {
                notificationLabel.text = text;
            }
        }

    }

}
