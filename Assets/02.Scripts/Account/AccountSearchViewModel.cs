using System;

public class AccountSearchViewModel : ViewModelBase
{
    private AccountSearchService _service;

    public event Action OnCompleteSearch;
    public event Action OnFailSearch;

    private string _inputId = "";
    public string InputId
    {
        get
        {
            return _inputId;
        }
        set
        {
            if (_inputId != value)
            {
                _inputId = value;
                OnPropertyChanged(nameof(InputId));
            }
        }
    }

    private long _targetUserUid = 0;
    public long TargetUserUid
    {
        get
        {
            return _targetUserUid;
        }
        set
        {
            if (_targetUserUid != value)
            {
                _targetUserUid = value;
                OnPropertyChanged(nameof(TargetUserUid));
            }
        }
    }

    public void SetService(AccountSearchService service)
    {
        _service = service;
    }

    public async void RequestSearch()
    {
        if (_service == null) return;

        long resultUid = await _service.TrySearchAccountAsync(_inputId);

        if (resultUid != 0)
        {
            TargetUserUid = resultUid;
            InvokeCompleteSearch();
        }
        else
        {
            InvokeFailSearch();
        }
    }

    private void InvokeCompleteSearch()
    {
        if (OnCompleteSearch != null)
        {
            OnCompleteSearch.Invoke();
        }
    }

    private void InvokeFailSearch()
    {
        if (OnFailSearch != null)
        {
            OnFailSearch.Invoke();
        }
    }
}