//
// AzureSpatialAnchors
// This file was auto-generated from SscApiModelDirect.cs.
//

#pragma once

#include <string>
#include <vector>
#include <unordered_map>
#include <functional>
#include <memory>
#include <mutex>
#include <windows.h>
#include <winrt/Windows.Perception.Spatial.h>

namespace Microsoft { namespace Azure { namespace SpatialAnchors
{
    struct event_token
    {
        int64_t value{};

        explicit operator bool() const noexcept
        {
            return value != 0;
        }
    };

    inline bool operator==(event_token const& left, event_token const& right) noexcept
    {
        return left.value == right.value;
    }

    template <typename Delegate>
    struct event
    {
        using delegate_type = Delegate;

        event() = default;
        event(event<Delegate> const&) = delete;
        event<Delegate>& operator =(event<Delegate> const&) = delete;

        event_token add(delegate_type const& delegate)
        {
            event_token token{};

            {
                std::unique_lock<std::mutex> lock(m_change);

                token = get_next_token();
                m_targets.emplace(token.value, delegate);
            }

            return token;
        }

        void remove(event_token const token)
        {
            {
                std::unique_lock<std::mutex> lock(m_change);
                m_targets.erase(token.value);
            }
        }

        template<typename...Arg>
        void operator()(Arg const&... args)
        {
            delegate_array temp_targets;
            {
                std::unique_lock<std::mutex> lock(m_change);
                temp_targets = m_targets;
            }

            for (auto const& element : temp_targets)
            {
                element.second(args...);
            }
        }

    private:

        event_token get_next_token() noexcept
        {
            return event_token{ m_next_token++ };
        }

        using delegate_array = std::unordered_map<int64_t, delegate_type>;

        int64_t m_next_token{0};
        delegate_array m_targets;
        std::mutex m_change;
    };

    template <typename T>
    struct fast_iterator
    {
        using iterator_category = std::input_iterator_tag;
        using value_type = T;
        using difference_type = ptrdiff_t;
        using pointer = T * ;
        using reference = T & ;

        fast_iterator(T const& collection, uint32_t const index) noexcept :
                m_collection(&collection),
                m_index(index)
        {}

        fast_iterator& operator++() noexcept
        {
            ++m_index;
            return*this;
        }

        auto operator*() const
        {
            return m_collection->GetAt(m_index);
        }

        bool operator==(fast_iterator const& other) const noexcept
        {
            return m_collection == other.m_collection && m_index == other.m_index;
        }

        bool operator!=(fast_iterator const& other) const noexcept
        {
            return !(*this == other);
        }

    private:

        T const* m_collection = nullptr;
        uint32_t m_index = 0;
    };

    template <typename T>
    fast_iterator<T> begin(T const& collection) noexcept
    {
        return fast_iterator<T>(collection, 0);
    }

    template <typename T>
    fast_iterator<T> end(T const& collection)
    {
        return fast_iterator<T>(collection, collection.Size());
    }

    template <typename T>
    struct IVector
    {
        virtual T GetAt(uint32_t const index) const = 0;
        virtual void SetAt(uint32_t const index, T const& value) = 0;
        virtual void InsertAt(uint32_t const index, T const& value) = 0;
        virtual void Append(T const& value) = 0;
        virtual void RemoveAt(uint32_t const index) = 0;
        virtual void Clear() = 0;
        virtual uint32_t Size() const = 0;
    };

    template <typename K, typename V>
    struct key_value_pair
    {
        key_value_pair(K key, V value) :
            m_key(std::move(key)),
            m_value(std::move(value))
        {
        }

        K Key() const
        {
            return m_key;
        }

        V Value() const
        {
            return m_value;
        }

    private:

        K const m_key;
        V const m_value;
    };

    template <typename K, typename V>
    struct IMap
    {
        virtual V Lookup(K const& key) const = 0;
        virtual void Clear() = 0;
        virtual uint32_t Size() const = 0;
        virtual key_value_pair<K, V> GetAt(uint32_t index) const = 0;
        virtual bool HasKey(K const& key) const = 0;
        virtual bool Insert(K const& key, V const& value) = 0;
        virtual void Remove(K const& key) = 0;
    };

    enum class Status : int32_t
    {
        OK = 0,
        Failed = 1,
        ObjectDisposed = 2,
        OutOfMemory = 12,
        InvalidArgument = 22,
        OutOfRange = 34,
        NotImplemented = 38,
        KeyNotFound = 77,
        MetadataTooLarge = 78,
        ApplicationNotAuthenticated = 79,
        ApplicationNotAuthorized = 80,
        ConcurrencyViolation = 81,
        NotEnoughSpatialData = 82,
        NoSpatialLocationHint = 83,
        CannotConnectToServer = 84,
        ServerError = 85,
        AlreadyAssociatedWithADifferentStore = 86,
        AlreadyExists = 87,
        NoLocateCriteriaSpecified = 88,
        NoAccessTokenSpecified = 89,
        UnableToObtainAccessToken = 90,
        TooManyRequests = 91,
        LocateCriteriaMissingRequiredValues = 92,
        LocateCriteriaInConflict = 93,
        LocateCriteriaInvalid = 94,
        LocateCriteriaNotSupported = 95,
        Unknown = 96,
    };

    enum class SessionLogLevel : int32_t
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Information = 3,
        Debug = 4,
        All = 5,
    };

    enum class LocateAnchorStatus : int32_t
    {
        AlreadyTracked = 0,
        Located = 1,
        NotLocated = 2,
        NotLocatedAnchorDoesNotExist = 3,
    };

    enum class LocateStrategy : int32_t
    {
        AnyStrategy = 0,
        VisualInformation = 1,
        Relationship = 2,
    };

    enum class SessionUserFeedback : int32_t
    {
        None = 0,
        NotEnoughMotion = 1,
        MotionTooQuick = 2,
        NotEnoughFeatures = 4,
    };

    enum class CloudSpatialErrorCode : int32_t
    {
        MetadataTooLarge = 0,
        ApplicationNotAuthenticated = 1,
        ApplicationNotAuthorized = 2,
        ConcurrencyViolation = 3,
        NotEnoughSpatialData = 4,
        NoSpatialLocationHint = 5,
        CannotConnectToServer = 6,
        ServerError = 7,
        AlreadyAssociatedWithADifferentStore = 8,
        AlreadyExists = 9,
        NoLocateCriteriaSpecified = 10,
        NoAccessTokenSpecified = 11,
        UnableToObtainAccessToken = 12,
        TooManyRequests = 13,
        LocateCriteriaMissingRequiredValues = 14,
        LocateCriteriaInConflict = 15,
        LocateCriteriaInvalid = 16,
        LocateCriteriaNotSupported = 17,
        Unknown = 18,
    };

    enum class AnchorDataCategory : int32_t
    {
        None = 0,
        Properties = 1,
        Spatial = 2,
    };

    struct AnchorLocateCriteria;
    struct AnchorLocatedEventArgs;
    struct CloudSpatialAnchor;
    struct CloudSpatialAnchorSessionDeferral;
    struct CloudSpatialAnchorSessionDiagnostics;
    struct CloudSpatialAnchorSession;
    struct CloudSpatialAnchorWatcher;
    struct LocateAnchorsCompletedEventArgs;
    struct NearAnchorCriteria;
    struct OnLogDebugEventArgs;
    struct SessionConfiguration;
    struct SessionErrorEventArgs;
    struct SessionStatus;
    struct SessionUpdatedEventArgs;
    struct TokenRequiredEventArgs;

    using LocateAnchorsCompletedDelegate = std::function<void(void*, const std::shared_ptr<LocateAnchorsCompletedEventArgs> &)>;
    using TokenRequiredDelegate = std::function<void(void*, const std::shared_ptr<TokenRequiredEventArgs> &)>;
    using AnchorLocatedDelegate = std::function<void(void*, const std::shared_ptr<AnchorLocatedEventArgs> &)>;
    using SessionUpdatedDelegate = std::function<void(void*, const std::shared_ptr<SessionUpdatedEventArgs> &)>;
    using SessionErrorDelegate = std::function<void(void*, const std::shared_ptr<SessionErrorEventArgs> &)>;
    using OnLogDebugDelegate = std::function<void(void*, const std::shared_ptr<OnLogDebugEventArgs> &)>;

    struct AnchorLocateCriteria : std::enable_shared_from_this<AnchorLocateCriteria>
    {
        AnchorLocateCriteria();
        AnchorLocateCriteria(void* handle, bool noCopy = true);
        ~AnchorLocateCriteria();

        std::vector<std::string> Identifiers();
        void Identifiers(std::vector<std::string> const& value);
        bool BypassCache();
        void BypassCache(bool const& value);
        std::shared_ptr<NearAnchorCriteria> NearAnchor();
        void NearAnchor(std::shared_ptr<NearAnchorCriteria> const& value);
        AnchorDataCategory RequestedCategories();
        void RequestedCategories(AnchorDataCategory const& value);
        LocateStrategy Strategy();
        void Strategy(LocateStrategy const& value);

        void* Handle() const;

    private:
        void* m_handle;
        std::vector<std::string> m_identifiers;
    };

    struct AnchorLocatedEventArgs : std::enable_shared_from_this<AnchorLocatedEventArgs>
    {
        AnchorLocatedEventArgs(void* handle, bool noCopy = true);
        ~AnchorLocatedEventArgs();

        std::shared_ptr<CloudSpatialAnchor> Anchor();
        std::string Identifier();
        LocateAnchorStatus Status();
        LocateStrategy Strategy();
        std::shared_ptr<CloudSpatialAnchorWatcher> Watcher();

        void* Handle() const;

    private:
        void* m_handle;
    };

    struct CloudSpatialAnchor : std::enable_shared_from_this<CloudSpatialAnchor>
    {
        CloudSpatialAnchor();
        CloudSpatialAnchor(void* handle, bool noCopy = true);
        ~CloudSpatialAnchor();

        winrt::Windows::Perception::Spatial::SpatialAnchor LocalAnchor();
        void LocalAnchor(winrt::Windows::Perception::Spatial::SpatialAnchor const& value);
        int64_t Expiration();
        void Expiration(int64_t const& value);
        std::string Identifier();
        std::shared_ptr<IMap<std::string, std::string>> AppProperties();
        std::string VersionTag();

        void* Handle() const;

    private:
        void* m_handle;
    };

    struct CloudSpatialAnchorSessionDeferral : std::enable_shared_from_this<CloudSpatialAnchorSessionDeferral>
    {
        CloudSpatialAnchorSessionDeferral(void* handle, bool noCopy = true);
        ~CloudSpatialAnchorSessionDeferral();

        void Complete();

        void* Handle() const;

    private:
        void* m_handle;
    };

    struct CloudSpatialAnchorSessionDiagnostics : std::enable_shared_from_this<CloudSpatialAnchorSessionDiagnostics>
    {
        CloudSpatialAnchorSessionDiagnostics(void* handle, bool noCopy = true);
        ~CloudSpatialAnchorSessionDiagnostics();

        SessionLogLevel LogLevel();
        void LogLevel(SessionLogLevel const& value);
        std::string LogDirectory();
        void LogDirectory(std::string const& value);
        int32_t MaxDiskSizeInMB();
        void MaxDiskSizeInMB(int32_t const& value);
        bool ImagesEnabled();
        void ImagesEnabled(bool const& value);
        void CreateManifestAsync(std::string const & description, std::function<void(Status, const std::string &)> callback);
        void SubmitManifestAsync(std::string const & manifestPath, std::function<void(Status)> callback);

        void* Handle() const;

    private:
        void* m_handle;
    };

    struct CloudSpatialAnchorSession : std::enable_shared_from_this<CloudSpatialAnchorSession>
    {
        CloudSpatialAnchorSession();
        CloudSpatialAnchorSession(void* handle, bool noCopy = true);
        ~CloudSpatialAnchorSession();

        std::shared_ptr<SessionConfiguration> Configuration();
        std::shared_ptr<CloudSpatialAnchorSessionDiagnostics> Diagnostics();
        SessionLogLevel LogLevel();
        void LogLevel(SessionLogLevel const& value);
        std::string SessionId();
        event_token TokenRequired(TokenRequiredDelegate const& handler);
        void TokenRequired(event_token const& token);
        event_token AnchorLocated(AnchorLocatedDelegate const& handler);
        void AnchorLocated(event_token const& token);
        event_token LocateAnchorsCompleted(LocateAnchorsCompletedDelegate const& handler);
        void LocateAnchorsCompleted(event_token const& token);
        event_token SessionUpdated(SessionUpdatedDelegate const& handler);
        void SessionUpdated(event_token const& token);
        event_token Error(SessionErrorDelegate const& handler);
        void Error(event_token const& token);
        event_token OnLogDebug(OnLogDebugDelegate const& handler);
        void OnLogDebug(event_token const& token);
        void Dispose();
        void GetAccessTokenWithAuthenticationTokenAsync(std::string const & authenticationToken, std::function<void(Status, const std::string &)> callback);
        void GetAccessTokenWithAccountKeyAsync(std::string const & accountKey, std::function<void(Status, const std::string &)> callback);
        void CreateAnchorAsync(std::shared_ptr<CloudSpatialAnchor> const & anchor, std::function<void(Status)> callback);
        std::shared_ptr<CloudSpatialAnchorWatcher> CreateWatcher(std::shared_ptr<AnchorLocateCriteria> const & criteria);
        void GetAnchorPropertiesAsync(std::string const & identifier, std::function<void(Status, const std::shared_ptr<CloudSpatialAnchor> &)> callback);
        std::vector<std::shared_ptr<CloudSpatialAnchorWatcher>> GetActiveWatchers();
        void RefreshAnchorPropertiesAsync(std::shared_ptr<CloudSpatialAnchor> const & anchor, std::function<void(Status)> callback);
        void UpdateAnchorPropertiesAsync(std::shared_ptr<CloudSpatialAnchor> const & anchor, std::function<void(Status)> callback);
        void DeleteAnchorAsync(std::shared_ptr<CloudSpatialAnchor> const & anchor, std::function<void(Status)> callback);
        void GetSessionStatusAsync(std::function<void(Status, const std::shared_ptr<SessionStatus> &)> callback);
        void Start();
        void Stop();
        void Reset();

        void* Handle() const;

    private:
        void* m_handle;

        event<TokenRequiredDelegate> m_tokenRequiredEvent;
        event<AnchorLocatedDelegate> m_anchorLocatedEvent;
        event<LocateAnchorsCompletedDelegate> m_locateAnchorsCompletedEvent;
        event<SessionUpdatedDelegate> m_sessionUpdatedEvent;
        event<SessionErrorDelegate> m_errorEvent;
        event<OnLogDebugDelegate> m_onLogDebugEvent;
    };

    struct CloudSpatialAnchorWatcher : std::enable_shared_from_this<CloudSpatialAnchorWatcher>
    {
        CloudSpatialAnchorWatcher(void* handle, bool noCopy = true);
        ~CloudSpatialAnchorWatcher();

        int32_t Identifier();
        void Stop();

        void* Handle() const;

    private:
        void* m_handle;
    };

    struct LocateAnchorsCompletedEventArgs : std::enable_shared_from_this<LocateAnchorsCompletedEventArgs>
    {
        LocateAnchorsCompletedEventArgs(void* handle, bool noCopy = true);
        ~LocateAnchorsCompletedEventArgs();

        bool Cancelled();
        std::shared_ptr<CloudSpatialAnchorWatcher> Watcher();

        void* Handle() const;

    private:
        void* m_handle;
    };

    struct NearAnchorCriteria : std::enable_shared_from_this<NearAnchorCriteria>
    {
        NearAnchorCriteria();
        NearAnchorCriteria(void* handle, bool noCopy = true);
        ~NearAnchorCriteria();

        std::shared_ptr<CloudSpatialAnchor> SourceAnchor();
        void SourceAnchor(std::shared_ptr<CloudSpatialAnchor> const& value);
        float DistanceInMeters();
        void DistanceInMeters(float const& value);
        int32_t MaxResultCount();
        void MaxResultCount(int32_t const& value);

        void* Handle() const;

    private:
        void* m_handle;
    };

    struct OnLogDebugEventArgs : std::enable_shared_from_this<OnLogDebugEventArgs>
    {
        OnLogDebugEventArgs(void* handle, bool noCopy = true);
        ~OnLogDebugEventArgs();

        std::string Message();

        void* Handle() const;

    private:
        void* m_handle;
    };

    struct SessionConfiguration : std::enable_shared_from_this<SessionConfiguration>
    {
        SessionConfiguration(void* handle, bool noCopy = true);
        ~SessionConfiguration();

        std::string AccountDomain();
        void AccountDomain(std::string const& value);
        std::string AccountId();
        void AccountId(std::string const& value);
        std::string AuthenticationToken();
        void AuthenticationToken(std::string const& value);
        std::string AccountKey();
        void AccountKey(std::string const& value);
        std::string AccessToken();
        void AccessToken(std::string const& value);

        void* Handle() const;

    private:
        void* m_handle;
    };

    struct SessionErrorEventArgs : std::enable_shared_from_this<SessionErrorEventArgs>
    {
        SessionErrorEventArgs(void* handle, bool noCopy = true);
        ~SessionErrorEventArgs();

        CloudSpatialErrorCode ErrorCode();
        std::string ErrorMessage();
        std::shared_ptr<CloudSpatialAnchorWatcher> Watcher();

        void* Handle() const;

    private:
        void* m_handle;
    };

    struct SessionStatus : std::enable_shared_from_this<SessionStatus>
    {
        SessionStatus(void* handle, bool noCopy = true);
        ~SessionStatus();

        float ReadyForCreateProgress();
        float RecommendedForCreateProgress();
        int32_t SessionCreateHash();
        int32_t SessionLocateHash();
        SessionUserFeedback UserFeedback();

        void* Handle() const;

    private:
        void* m_handle;
    };

    struct SessionUpdatedEventArgs : std::enable_shared_from_this<SessionUpdatedEventArgs>
    {
        SessionUpdatedEventArgs(void* handle, bool noCopy = true);
        ~SessionUpdatedEventArgs();

        std::shared_ptr<SessionStatus> Status();

        void* Handle() const;

    private:
        void* m_handle;
    };

    struct TokenRequiredEventArgs : std::enable_shared_from_this<TokenRequiredEventArgs>
    {
        TokenRequiredEventArgs(void* handle, bool noCopy = true);
        ~TokenRequiredEventArgs();

        std::string AccessToken();
        void AccessToken(std::string const& value);
        std::string AuthenticationToken();
        void AuthenticationToken(std::string const& value);
        std::shared_ptr<CloudSpatialAnchorSessionDeferral> GetDeferral();

        void* Handle() const;

    private:
        void* m_handle;
    };

    struct runtime_error
    {
        runtime_error() noexcept(noexcept(std::string())) :
            m_status(Status::Failed)
        {
        }

        runtime_error(runtime_error&&) = default;
        runtime_error& operator=(runtime_error&&) = default;

        runtime_error(runtime_error const& other) noexcept :
            m_status(other.m_status),
            m_message(other.m_message),
            m_requestCorrelationVector(other.m_requestCorrelationVector),
            m_responseCorrelationVector(other.m_responseCorrelationVector)
        {
        }

        runtime_error& operator=(runtime_error const& other) noexcept
        {
            m_status = other.m_status;
            m_message = other.m_message;
            m_requestCorrelationVector = other.m_requestCorrelationVector;
            m_responseCorrelationVector = other.m_responseCorrelationVector;
            return *this;
        }

        runtime_error(Status const status, std::string const& message, std::string const& requestCorrelationVector, std::string const& responseCorrelationVector) noexcept :
            m_status(status),
            m_message(std::move(message)),
            m_requestCorrelationVector(std::move(requestCorrelationVector)),
            m_responseCorrelationVector(std::move(responseCorrelationVector))
        {
        }

        runtime_error(Status const status, std::string const& message) noexcept
            : m_status(status),
              m_message(std::move(message))
        {
        }

        runtime_error(Status const status) noexcept(noexcept(std::string())) :
            m_status(status)
        {
        }

        Status status() const noexcept
        {
            return m_status;
        }

        const std::string& message() const noexcept
        {
            return m_message;
        }

        const std::string& requestCorrelationVector() const noexcept
        {
            return m_requestCorrelationVector;
        }

        const std::string& responseCorrelationVector() const noexcept
        {
            return m_responseCorrelationVector;
        }

    private:
        Status m_status;
        std::string m_message;
        std::string m_requestCorrelationVector;
        std::string m_responseCorrelationVector;
    };
} } } 

