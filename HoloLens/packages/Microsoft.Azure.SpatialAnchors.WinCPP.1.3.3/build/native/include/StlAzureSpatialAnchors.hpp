//
// AzureSpatialAnchors
// This file was auto-generated from SscApiModelDirect.cs.
//

#include <thread>
#include <vector>
#include <sstream>
#include <algorithm>
#include <atomic>

extern "C" {
#include <AzureSpatialAnchorsLibrary_Exports.h>
}

#ifdef min
#undef min
#endif

namespace
{
    using namespace Microsoft::Azure::SpatialAnchors;

    void check_status(void* handle, ssc_status value)
    {
        if (value == ssc_status_ok)
        {
            return;
        }

        const char * outParam_message = nullptr;
        const char * outParam_requestCorrelationVector = nullptr;
        const char * outParam_responseCorrelationVector = nullptr;

        ssc_status status = ssc_get_error_details(handle, &outParam_message, &outParam_requestCorrelationVector, &outParam_responseCorrelationVector);

        std::string fullMessage;
        if (status == ssc_status_failed)
        {
            throw runtime_error(static_cast<Status>(status), "Unexpected error in exception handling.");
        }
        else if (status != ssc_status_ok)
        {
            fullMessage = "Exception thrown and an unexpected error in exception handling.";
        }
        else
        {
            std::ostringstream fullMessageStr;
            fullMessageStr << "Message: " << outParam_message << ".Request CV: " << outParam_requestCorrelationVector << ".Response CV: " << outParam_responseCorrelationVector << ".";
            fullMessage = fullMessageStr.str();
        }

        switch (value)
        {
            case ssc_status_ok: return;
            case ssc_status_out_of_memory: throw std::bad_alloc();
            case ssc_status_out_of_range: throw std::out_of_range(fullMessage);
            case ssc_status_invalid_arg: throw std::invalid_argument(fullMessage);
            default:
                throw runtime_error(static_cast<Status>(value), outParam_message, outParam_requestCorrelationVector, outParam_responseCorrelationVector);
        }
     }

    template<typename T>
    class CookieTracker
    {
    public:
        ssc_callback_cookie getCookie(const std::shared_ptr<T>& instance)
        {
            auto pair = m_tracked.emplace(reinterpret_cast<ssc_callback_cookie>(instance.get()), instance);
            return pair.first->first;
        }

        std::shared_ptr<T> lookup(ssc_callback_cookie cookie)
        {
            auto itr = m_tracked.find(cookie);
            if (itr == m_tracked.end())
            {
                return nullptr;
            }
            auto result = itr->second.lock();
            if (result == nullptr)
            {
                m_tracked.erase(cookie);
            }
            return result;
        }

        void remove(T* instance)
        {
            m_tracked.erase(reinterpret_cast<ssc_callback_cookie>(instance));
        }

    private:
        std::unordered_map<ssc_callback_cookie, std::weak_ptr<T>> m_tracked;
    };

    struct IDictionary_String_String : IMap<std::string, std::string>
    {
        IDictionary_String_String(ssc_idictionary_string_string_handle handle, bool noCopy = true) : m_handle(handle)
        {
            if (!noCopy)
            {
                check_status(m_handle, ssc_idictionary_string_string_addref(m_handle));
            }
        }

        ~IDictionary_String_String()
        {
            check_status(m_handle, ssc_idictionary_string_string_release(m_handle));
        }

        std::string Lookup(std::string const& key) const override
        {
            const char * outParam;
            auto resultCode = ssc_idictionary_string_string_get_item(m_handle, key.c_str(), &outParam);
            if (resultCode == ssc_status_key_not_found)
            {
                throw std::out_of_range("");
            }
            else
            {
                check_status(m_handle, resultCode);
            }
            std::string result = outParam; CoTaskMemFree(const_cast<char*>(outParam));

            return result;
        }

        uint32_t Size() const override
        {
            int size;
            check_status(m_handle, ssc_idictionary_string_string_get_count(m_handle, &size));

            return size;
        }

        bool HasKey(std::string const& key) const override
        {
            const char * outParam;
            auto resultCode = ssc_idictionary_string_string_get_item(m_handle, key.c_str(), &outParam);
            auto success = resultCode == ssc_status_ok;
            std::string result = outParam; CoTaskMemFree(const_cast<char*>(outParam));

            return success;
        }

        bool Insert(std::string const& key, std::string const& value) override
        {
            auto result = HasKey(key);
            check_status(m_handle, ssc_idictionary_string_string_set_item(m_handle, key.c_str(), value.c_str()));
            return result;
        }

        void Remove(std::string const& key) override
        {
            check_status(m_handle, ssc_idictionary_string_string_remove_key(m_handle, key.c_str()));
        }

        void Clear() override
        {
            check_status(m_handle, ssc_idictionary_string_string_clear(m_handle));
        }

        key_value_pair<std::string, std::string> GetAt(uint32_t index) const override
        {
            if (index >= Size())
            {
                throw std::out_of_range("");
            }

            const char * outParam;
            check_status(m_handle, ssc_idictionary_string_string_get_key(m_handle, index, &outParam));
            std::string key = outParam; CoTaskMemFree(const_cast<char*>(outParam));
            auto value = Lookup(key);

            auto result = key_value_pair<std::string, std::string>(key, value);

            return result;
        }

    private:
        ssc_idictionary_string_string_handle m_handle;
    };

    static std::vector<std::shared_ptr<CloudSpatialAnchorWatcher>> create_vector_CloudSpatialAnchorWatcher(ssc_cloud_spatial_anchor_watcher_handle* values, size_t len)
    {
        ::std::vector<std::shared_ptr<CloudSpatialAnchorWatcher>> v;
        v.reserve(len);
        for (size_t i = 0; i < len; ++i)
        {
            v.push_back(std::make_shared<CloudSpatialAnchorWatcher>(values[i]));
        }
        return v;
    }

    struct thread_pool
    {
        void submit_work(const std::function<void(void)> &&work)
        {
            {
                std::unique_lock<std::mutex> lock(m_mutex);
                m_work.emplace_back(work);
            }
            m_condition.notify_one();
        }

        static thread_pool& get()
        {
            std::call_once(s_poolStarted, []() { s_threadPool.start(); });
            return s_threadPool;
        }

    private:
        thread_pool()
        {
        }

        void start()
        {
            auto threadCount = std::min(4U, std::thread::hardware_concurrency());
            for (size_t i = 0; i < threadCount; i++)
            {
                m_threads.emplace_back([this]()
                {
                    while (!m_stopRequested)
                    {
                        std::vector<std::function<void(void)>> temp_work;
                        {
                            std::unique_lock<std::mutex> lock(m_mutex);
                            m_condition.wait(lock, [this]{ return m_stopRequested || !m_work.empty(); });

                            if (m_stopRequested)
                            {
                                break;
                            }

                            temp_work = std::move(m_work);
                        }

                        for (auto const& work : temp_work)
                        {
                            work();
                        }
                    }
                });
            }
        }

        ~thread_pool()
        {
            {
                std::unique_lock<std::mutex> lock(m_mutex);
                m_stopRequested = true;
            }
            m_condition.notify_all();
            for (auto& thread : m_threads)
            {
                thread.join();
            }
            m_threads.clear();
        }

    private:
        std::vector<std::thread> m_threads;
        std::condition_variable m_condition;
        std::mutex m_mutex;
        std::vector<std::function<void(void)>> m_work;
        std::atomic_bool m_stopRequested{ false };
        static thread_pool s_threadPool;
        static std::once_flag s_poolStarted;
    };

    thread_pool thread_pool::s_threadPool;
    std::once_flag thread_pool::s_poolStarted;
}

namespace Microsoft { namespace Azure { namespace SpatialAnchors
{
    AnchorLocateCriteria::AnchorLocateCriteria(void* handle, bool noCopy)
        : m_handle(handle)
    {
        if (!noCopy)
        {
            check_status(m_handle, ssc_anchor_locate_criteria_addref(static_cast<ssc_anchor_locate_criteria_handle>(m_handle)));
        }
    }

    AnchorLocateCriteria::~AnchorLocateCriteria()
    {
        check_status(m_handle, ssc_anchor_locate_criteria_release(static_cast<ssc_anchor_locate_criteria_handle>(m_handle)));
    }

    void* AnchorLocateCriteria::Handle() const
    {
        return m_handle;
    }

    AnchorLocateCriteria::AnchorLocateCriteria()
    {
        ssc_status resultStatus = ssc_anchor_locate_criteria_create(reinterpret_cast<ssc_anchor_locate_criteria_handle*>(&m_handle));
        check_status(m_handle, resultStatus);
    }

    std::vector<std::string> AnchorLocateCriteria::Identifiers()
    {
        const char * * outParam_result;
        int result_length = 0;
        check_status(m_handle, ssc_anchor_locate_criteria_get_identifiers(static_cast<ssc_anchor_locate_criteria_handle>(m_handle), &outParam_result, &result_length));
        std::vector<std::string> result_localArray_tmp;
        for (int i = 0; i < result_length; i++)
        {
            std::string itrValue = outParam_result[i]; CoTaskMemFree(const_cast<char*>(outParam_result[i]));
            result_localArray_tmp.emplace_back(itrValue);
        }
        CoTaskMemFree(const_cast<const char * *>(outParam_result));
        auto result = std::move(result_localArray_tmp);
        return result;
    }

    void AnchorLocateCriteria::Identifiers(std::vector<std::string> const& value)
    {
        std::vector<const char *> value_localArray;
        for (const auto& itrValue : value)
        {
            value_localArray.emplace_back(itrValue.c_str());
        }
        check_status(m_handle, ssc_anchor_locate_criteria_set_identifiers(static_cast<ssc_anchor_locate_criteria_handle>(m_handle), value_localArray.data(), static_cast<int>(value_localArray.size())));
    }

    bool AnchorLocateCriteria::BypassCache()
    {
        char outParam_result;
        check_status(m_handle, ssc_anchor_locate_criteria_get_bypass_cache(static_cast<ssc_anchor_locate_criteria_handle>(m_handle), &outParam_result));
        bool result = static_cast<bool>(outParam_result);
        return result;
    }

    void AnchorLocateCriteria::BypassCache(bool const& value)
    {
        check_status(m_handle, ssc_anchor_locate_criteria_set_bypass_cache(static_cast<ssc_anchor_locate_criteria_handle>(m_handle), static_cast<bool>(value)));
    }

    std::shared_ptr<NearAnchorCriteria> AnchorLocateCriteria::NearAnchor()
    {
        ssc_near_anchor_criteria_handle outParam_result;
        check_status(m_handle, ssc_anchor_locate_criteria_get_near_anchor(static_cast<ssc_anchor_locate_criteria_handle>(m_handle), &outParam_result));
        std::shared_ptr<NearAnchorCriteria> result = outParam_result ? std::make_shared<NearAnchorCriteria>(outParam_result) : nullptr;
        return result;
    }

    void AnchorLocateCriteria::NearAnchor(std::shared_ptr<NearAnchorCriteria> const& value)
    {
        check_status(m_handle, ssc_anchor_locate_criteria_set_near_anchor(static_cast<ssc_anchor_locate_criteria_handle>(m_handle), static_cast<ssc_near_anchor_criteria_handle>(value->Handle())));
    }

    AnchorDataCategory AnchorLocateCriteria::RequestedCategories()
    {
        ssc_anchor_data_category outParam_result;
        check_status(m_handle, ssc_anchor_locate_criteria_get_requested_categories(static_cast<ssc_anchor_locate_criteria_handle>(m_handle), &outParam_result));
        auto result = static_cast<AnchorDataCategory>(outParam_result);
        return result;
    }

    void AnchorLocateCriteria::RequestedCategories(AnchorDataCategory const& value)
    {
        check_status(m_handle, ssc_anchor_locate_criteria_set_requested_categories(static_cast<ssc_anchor_locate_criteria_handle>(m_handle), static_cast<ssc_anchor_data_category>(value)));
    }

    LocateStrategy AnchorLocateCriteria::Strategy()
    {
        ssc_locate_strategy outParam_result;
        check_status(m_handle, ssc_anchor_locate_criteria_get_strategy(static_cast<ssc_anchor_locate_criteria_handle>(m_handle), &outParam_result));
        auto result = static_cast<LocateStrategy>(outParam_result);
        return result;
    }

    void AnchorLocateCriteria::Strategy(LocateStrategy const& value)
    {
        check_status(m_handle, ssc_anchor_locate_criteria_set_strategy(static_cast<ssc_anchor_locate_criteria_handle>(m_handle), static_cast<ssc_locate_strategy>(value)));
    }

    AnchorLocatedEventArgs::AnchorLocatedEventArgs(void* handle, bool noCopy)
        : m_handle(handle)
    {
        if (!noCopy)
        {
            check_status(m_handle, ssc_anchor_located_event_args_addref(static_cast<ssc_anchor_located_event_args_handle>(m_handle)));
        }
    }

    AnchorLocatedEventArgs::~AnchorLocatedEventArgs()
    {
        check_status(m_handle, ssc_anchor_located_event_args_release(static_cast<ssc_anchor_located_event_args_handle>(m_handle)));
    }

    void* AnchorLocatedEventArgs::Handle() const
    {
        return m_handle;
    }

    std::shared_ptr<CloudSpatialAnchor> AnchorLocatedEventArgs::Anchor()
    {
        ssc_cloud_spatial_anchor_handle outParam_result;
        check_status(m_handle, ssc_anchor_located_event_args_get_anchor(static_cast<ssc_anchor_located_event_args_handle>(m_handle), &outParam_result));
        std::shared_ptr<CloudSpatialAnchor> result = outParam_result ? std::make_shared<CloudSpatialAnchor>(outParam_result) : nullptr;
        return result;
    }

    std::string AnchorLocatedEventArgs::Identifier()
    {
        const char * outParam_result;
        check_status(m_handle, ssc_anchor_located_event_args_get_identifier(static_cast<ssc_anchor_located_event_args_handle>(m_handle), &outParam_result));
        std::string result = outParam_result; CoTaskMemFree(const_cast<char*>(outParam_result));
        return result;
    }

    LocateAnchorStatus AnchorLocatedEventArgs::Status()
    {
        ssc_locate_anchor_status outParam_result;
        check_status(m_handle, ssc_anchor_located_event_args_get_status(static_cast<ssc_anchor_located_event_args_handle>(m_handle), &outParam_result));
        auto result = static_cast<LocateAnchorStatus>(outParam_result);
        return result;
    }

    LocateStrategy AnchorLocatedEventArgs::Strategy()
    {
        ssc_locate_strategy outParam_result;
        check_status(m_handle, ssc_anchor_located_event_args_get_strategy(static_cast<ssc_anchor_located_event_args_handle>(m_handle), &outParam_result));
        auto result = static_cast<LocateStrategy>(outParam_result);
        return result;
    }

    std::shared_ptr<CloudSpatialAnchorWatcher> AnchorLocatedEventArgs::Watcher()
    {
        ssc_cloud_spatial_anchor_watcher_handle outParam_result;
        check_status(m_handle, ssc_anchor_located_event_args_get_watcher(static_cast<ssc_anchor_located_event_args_handle>(m_handle), &outParam_result));
        std::shared_ptr<CloudSpatialAnchorWatcher> result = outParam_result ? std::make_shared<CloudSpatialAnchorWatcher>(outParam_result) : nullptr;
        return result;
    }

    CloudSpatialAnchor::CloudSpatialAnchor(void* handle, bool noCopy)
        : m_handle(handle)
    {
        if (!noCopy)
        {
            check_status(m_handle, ssc_cloud_spatial_anchor_addref(static_cast<ssc_cloud_spatial_anchor_handle>(m_handle)));
        }
    }

    CloudSpatialAnchor::~CloudSpatialAnchor()
    {
        check_status(m_handle, ssc_cloud_spatial_anchor_release(static_cast<ssc_cloud_spatial_anchor_handle>(m_handle)));
    }

    void* CloudSpatialAnchor::Handle() const
    {
        return m_handle;
    }

    CloudSpatialAnchor::CloudSpatialAnchor()
    {
        ssc_status resultStatus = ssc_cloud_spatial_anchor_create(reinterpret_cast<ssc_cloud_spatial_anchor_handle*>(&m_handle));
        check_status(m_handle, resultStatus);
    }

    winrt::Windows::Perception::Spatial::SpatialAnchor CloudSpatialAnchor::LocalAnchor()
    {
        winrt::com_ptr<::IUnknown> outParam_result;
        check_status(m_handle, ssc_cloud_spatial_anchor_get_local_anchor(static_cast<ssc_cloud_spatial_anchor_handle>(m_handle), outParam_result.put()));
        winrt::Windows::Perception::Spatial::SpatialAnchor result = outParam_result.as<winrt::Windows::Perception::Spatial::SpatialAnchor>();
        return result;
    }

    void CloudSpatialAnchor::LocalAnchor(winrt::Windows::Perception::Spatial::SpatialAnchor const& value)
    {
        check_status(m_handle, ssc_cloud_spatial_anchor_set_local_anchor(static_cast<ssc_cloud_spatial_anchor_handle>(m_handle), winrt::get_unknown(value)));
    }

    int64_t CloudSpatialAnchor::Expiration()
    {
        int64_t outParam_result;
        check_status(m_handle, ssc_cloud_spatial_anchor_get_expiration(static_cast<ssc_cloud_spatial_anchor_handle>(m_handle), &outParam_result));
        int64_t result = outParam_result;
        return result;
    }

    void CloudSpatialAnchor::Expiration(int64_t const& value)
    {
        check_status(m_handle, ssc_cloud_spatial_anchor_set_expiration(static_cast<ssc_cloud_spatial_anchor_handle>(m_handle), value));
    }

    std::string CloudSpatialAnchor::Identifier()
    {
        const char * outParam_result;
        check_status(m_handle, ssc_cloud_spatial_anchor_get_identifier(static_cast<ssc_cloud_spatial_anchor_handle>(m_handle), &outParam_result));
        std::string result = outParam_result; CoTaskMemFree(const_cast<char*>(outParam_result));
        return result;
    }

    std::shared_ptr<IMap<std::string, std::string>> CloudSpatialAnchor::AppProperties()
    {
        ssc_idictionary_string_string_handle outParam_result;
        check_status(m_handle, ssc_cloud_spatial_anchor_get_app_properties(static_cast<ssc_cloud_spatial_anchor_handle>(m_handle), &outParam_result));
        std::shared_ptr<IMap<std::string, std::string>> result = std::make_shared<IDictionary_String_String>(outParam_result);
        return result;
    }

    std::string CloudSpatialAnchor::VersionTag()
    {
        const char * outParam_result;
        check_status(m_handle, ssc_cloud_spatial_anchor_get_version_tag(static_cast<ssc_cloud_spatial_anchor_handle>(m_handle), &outParam_result));
        std::string result = outParam_result; CoTaskMemFree(const_cast<char*>(outParam_result));
        return result;
    }

    CloudSpatialAnchorSessionDeferral::CloudSpatialAnchorSessionDeferral(void* handle, bool noCopy)
        : m_handle(handle)
    {
        if (!noCopy)
        {
            check_status(m_handle, ssc_cloud_spatial_anchor_session_deferral_addref(static_cast<ssc_cloud_spatial_anchor_session_deferral_handle>(m_handle)));
        }
    }

    CloudSpatialAnchorSessionDeferral::~CloudSpatialAnchorSessionDeferral()
    {
        check_status(m_handle, ssc_cloud_spatial_anchor_session_deferral_release(static_cast<ssc_cloud_spatial_anchor_session_deferral_handle>(m_handle)));
    }

    void* CloudSpatialAnchorSessionDeferral::Handle() const
    {
        return m_handle;
    }

    void CloudSpatialAnchorSessionDeferral::Complete()
    {
        check_status(m_handle, ssc_cloud_spatial_anchor_session_deferral_complete(static_cast<ssc_cloud_spatial_anchor_session_deferral_handle>(m_handle)));
    }

    CloudSpatialAnchorSessionDiagnostics::CloudSpatialAnchorSessionDiagnostics(void* handle, bool noCopy)
        : m_handle(handle)
    {
        if (!noCopy)
        {
            check_status(m_handle, ssc_cloud_spatial_anchor_session_diagnostics_addref(static_cast<ssc_cloud_spatial_anchor_session_diagnostics_handle>(m_handle)));
        }
    }

    CloudSpatialAnchorSessionDiagnostics::~CloudSpatialAnchorSessionDiagnostics()
    {
        check_status(m_handle, ssc_cloud_spatial_anchor_session_diagnostics_release(static_cast<ssc_cloud_spatial_anchor_session_diagnostics_handle>(m_handle)));
    }

    void* CloudSpatialAnchorSessionDiagnostics::Handle() const
    {
        return m_handle;
    }

    SessionLogLevel CloudSpatialAnchorSessionDiagnostics::LogLevel()
    {
        ssc_session_log_level outParam_result;
        check_status(m_handle, ssc_cloud_spatial_anchor_session_diagnostics_get_log_level(static_cast<ssc_cloud_spatial_anchor_session_diagnostics_handle>(m_handle), &outParam_result));
        auto result = static_cast<SessionLogLevel>(outParam_result);
        return result;
    }

    void CloudSpatialAnchorSessionDiagnostics::LogLevel(SessionLogLevel const& value)
    {
        check_status(m_handle, ssc_cloud_spatial_anchor_session_diagnostics_set_log_level(static_cast<ssc_cloud_spatial_anchor_session_diagnostics_handle>(m_handle), static_cast<ssc_session_log_level>(value)));
    }

    std::string CloudSpatialAnchorSessionDiagnostics::LogDirectory()
    {
        const char * outParam_result;
        check_status(m_handle, ssc_cloud_spatial_anchor_session_diagnostics_get_log_directory(static_cast<ssc_cloud_spatial_anchor_session_diagnostics_handle>(m_handle), &outParam_result));
        std::string result = outParam_result; CoTaskMemFree(const_cast<char*>(outParam_result));
        return result;
    }

    void CloudSpatialAnchorSessionDiagnostics::LogDirectory(std::string const& value)
    {
        check_status(m_handle, ssc_cloud_spatial_anchor_session_diagnostics_set_log_directory(static_cast<ssc_cloud_spatial_anchor_session_diagnostics_handle>(m_handle), value.c_str()));
    }

    int32_t CloudSpatialAnchorSessionDiagnostics::MaxDiskSizeInMB()
    {
        int outParam_result;
        check_status(m_handle, ssc_cloud_spatial_anchor_session_diagnostics_get_max_disk_size_in_mb(static_cast<ssc_cloud_spatial_anchor_session_diagnostics_handle>(m_handle), &outParam_result));
        int32_t result = outParam_result;
        return result;
    }

    void CloudSpatialAnchorSessionDiagnostics::MaxDiskSizeInMB(int32_t const& value)
    {
        check_status(m_handle, ssc_cloud_spatial_anchor_session_diagnostics_set_max_disk_size_in_mb(static_cast<ssc_cloud_spatial_anchor_session_diagnostics_handle>(m_handle), static_cast<int32_t>(value)));
    }

    bool CloudSpatialAnchorSessionDiagnostics::ImagesEnabled()
    {
        char outParam_result;
        check_status(m_handle, ssc_cloud_spatial_anchor_session_diagnostics_get_images_enabled(static_cast<ssc_cloud_spatial_anchor_session_diagnostics_handle>(m_handle), &outParam_result));
        bool result = static_cast<bool>(outParam_result);
        return result;
    }

    void CloudSpatialAnchorSessionDiagnostics::ImagesEnabled(bool const& value)
    {
        check_status(m_handle, ssc_cloud_spatial_anchor_session_diagnostics_set_images_enabled(static_cast<ssc_cloud_spatial_anchor_session_diagnostics_handle>(m_handle), static_cast<bool>(value)));
    }

    void CloudSpatialAnchorSessionDiagnostics::CreateManifestAsync(std::string const & param_description, std::function<void(Status, const std::string &)> callback)
    {
        auto self = shared_from_this();
        std::string description = param_description;
        thread_pool::get().submit_work([self, callback, description]
        {
            const char * outParam_result;
            auto status = (ssc_cloud_spatial_anchor_session_diagnostics_create_manifest_async(static_cast<ssc_cloud_spatial_anchor_session_diagnostics_handle>(self->m_handle), description.c_str(), &outParam_result));
            std::string result = outParam_result; CoTaskMemFree(const_cast<char*>(outParam_result));
            callback(static_cast<Status>(status), result);
        });
    }

    void CloudSpatialAnchorSessionDiagnostics::SubmitManifestAsync(std::string const & param_manifestPath, std::function<void(Status)> callback)
    {
        auto self = shared_from_this();
        std::string manifestPath = param_manifestPath;
        thread_pool::get().submit_work([self, callback, manifestPath]
        {
            auto status = (ssc_cloud_spatial_anchor_session_diagnostics_submit_manifest_async(static_cast<ssc_cloud_spatial_anchor_session_diagnostics_handle>(self->m_handle), manifestPath.c_str()));
            callback(static_cast<Status>(status));
        });
    }

    static CookieTracker<CloudSpatialAnchorSession> s_cloudSpatialAnchorSessionTracker;

    CloudSpatialAnchorSession::CloudSpatialAnchorSession(void* handle, bool noCopy)
        : m_handle(handle)
    {
        if (!noCopy)
        {
            check_status(m_handle, ssc_cloud_spatial_anchor_session_addref(static_cast<ssc_cloud_spatial_anchor_session_handle>(m_handle)));
        }
    }

    CloudSpatialAnchorSession::~CloudSpatialAnchorSession()
    {
        check_status(m_handle, ssc_cloud_spatial_anchor_session_release(static_cast<ssc_cloud_spatial_anchor_session_handle>(m_handle)));
        s_cloudSpatialAnchorSessionTracker.remove(this);
    }

    void* CloudSpatialAnchorSession::Handle() const
    {
        return m_handle;
    }

    CloudSpatialAnchorSession::CloudSpatialAnchorSession()
    {
        ssc_status resultStatus = ssc_cloud_spatial_anchor_session_create(reinterpret_cast<ssc_cloud_spatial_anchor_session_handle*>(&m_handle));
        check_status(m_handle, resultStatus);
        // Custom initialization begins for CloudSpatialAnchorSession.
        // Custom initialization ends for CloudSpatialAnchorSession.
    }

    std::shared_ptr<SessionConfiguration> CloudSpatialAnchorSession::Configuration()
    {
        ssc_session_configuration_handle outParam_result;
        check_status(m_handle, ssc_cloud_spatial_anchor_session_get_configuration(static_cast<ssc_cloud_spatial_anchor_session_handle>(m_handle), &outParam_result));
        std::shared_ptr<SessionConfiguration> result = outParam_result ? std::make_shared<SessionConfiguration>(outParam_result) : nullptr;
        return result;
    }

    std::shared_ptr<CloudSpatialAnchorSessionDiagnostics> CloudSpatialAnchorSession::Diagnostics()
    {
        ssc_cloud_spatial_anchor_session_diagnostics_handle outParam_result;
        check_status(m_handle, ssc_cloud_spatial_anchor_session_get_diagnostics(static_cast<ssc_cloud_spatial_anchor_session_handle>(m_handle), &outParam_result));
        std::shared_ptr<CloudSpatialAnchorSessionDiagnostics> result = outParam_result ? std::make_shared<CloudSpatialAnchorSessionDiagnostics>(outParam_result) : nullptr;
        return result;
    }

    SessionLogLevel CloudSpatialAnchorSession::LogLevel()
    {
        ssc_session_log_level outParam_result;
        check_status(m_handle, ssc_cloud_spatial_anchor_session_get_log_level(static_cast<ssc_cloud_spatial_anchor_session_handle>(m_handle), &outParam_result));
        auto result = static_cast<SessionLogLevel>(outParam_result);
        return result;
    }

    void CloudSpatialAnchorSession::LogLevel(SessionLogLevel const& value)
    {
        check_status(m_handle, ssc_cloud_spatial_anchor_session_set_log_level(static_cast<ssc_cloud_spatial_anchor_session_handle>(m_handle), static_cast<ssc_session_log_level>(value)));
    }

    std::string CloudSpatialAnchorSession::SessionId()
    {
        const char * outParam_result;
        check_status(m_handle, ssc_cloud_spatial_anchor_session_get_session_id(static_cast<ssc_cloud_spatial_anchor_session_handle>(m_handle), &outParam_result));
        std::string result = outParam_result; CoTaskMemFree(const_cast<char*>(outParam_result));
        return result;
    }

    event_token CloudSpatialAnchorSession::TokenRequired(TokenRequiredDelegate const& handler)
    {
        auto token = m_tokenRequiredEvent.add(handler);

        auto cookie = s_cloudSpatialAnchorSessionTracker.getCookie(shared_from_this());
        check_status(m_handle, ssc_cloud_spatial_anchor_session_set_token_required(static_cast<ssc_cloud_spatial_anchor_session_handle>(m_handle), cookie, [](ssc_callback_cookie cookie, ssc_token_required_event_args_handle args)
        {
            auto self = s_cloudSpatialAnchorSessionTracker.lookup(cookie);
            if (self != nullptr)
            {
                std::shared_ptr<TokenRequiredEventArgs> args_eventArg = args ? std::make_shared<TokenRequiredEventArgs>(args, false) : nullptr;
                self->m_tokenRequiredEvent(self.get(), args_eventArg);
            }
        }));
        return token;
    }

    void CloudSpatialAnchorSession::TokenRequired(event_token const& token)
    {
        m_tokenRequiredEvent.remove(token);
    }

    event_token CloudSpatialAnchorSession::AnchorLocated(AnchorLocatedDelegate const& handler)
    {
        auto token = m_anchorLocatedEvent.add(handler);

        auto cookie = s_cloudSpatialAnchorSessionTracker.getCookie(shared_from_this());
        check_status(m_handle, ssc_cloud_spatial_anchor_session_set_anchor_located(static_cast<ssc_cloud_spatial_anchor_session_handle>(m_handle), cookie, [](ssc_callback_cookie cookie, ssc_anchor_located_event_args_handle args)
        {
            auto self = s_cloudSpatialAnchorSessionTracker.lookup(cookie);
            if (self != nullptr)
            {
                std::shared_ptr<AnchorLocatedEventArgs> args_eventArg = args ? std::make_shared<AnchorLocatedEventArgs>(args, false) : nullptr;
                self->m_anchorLocatedEvent(self.get(), args_eventArg);
            }
        }));
        return token;
    }

    void CloudSpatialAnchorSession::AnchorLocated(event_token const& token)
    {
        m_anchorLocatedEvent.remove(token);
    }

    event_token CloudSpatialAnchorSession::LocateAnchorsCompleted(LocateAnchorsCompletedDelegate const& handler)
    {
        auto token = m_locateAnchorsCompletedEvent.add(handler);

        auto cookie = s_cloudSpatialAnchorSessionTracker.getCookie(shared_from_this());
        check_status(m_handle, ssc_cloud_spatial_anchor_session_set_locate_anchors_completed(static_cast<ssc_cloud_spatial_anchor_session_handle>(m_handle), cookie, [](ssc_callback_cookie cookie, ssc_locate_anchors_completed_event_args_handle args)
        {
            auto self = s_cloudSpatialAnchorSessionTracker.lookup(cookie);
            if (self != nullptr)
            {
                std::shared_ptr<LocateAnchorsCompletedEventArgs> args_eventArg = args ? std::make_shared<LocateAnchorsCompletedEventArgs>(args, false) : nullptr;
                self->m_locateAnchorsCompletedEvent(self.get(), args_eventArg);
            }
        }));
        return token;
    }

    void CloudSpatialAnchorSession::LocateAnchorsCompleted(event_token const& token)
    {
        m_locateAnchorsCompletedEvent.remove(token);
    }

    event_token CloudSpatialAnchorSession::SessionUpdated(SessionUpdatedDelegate const& handler)
    {
        auto token = m_sessionUpdatedEvent.add(handler);

        auto cookie = s_cloudSpatialAnchorSessionTracker.getCookie(shared_from_this());
        check_status(m_handle, ssc_cloud_spatial_anchor_session_set_session_updated(static_cast<ssc_cloud_spatial_anchor_session_handle>(m_handle), cookie, [](ssc_callback_cookie cookie, ssc_session_updated_event_args_handle args)
        {
            auto self = s_cloudSpatialAnchorSessionTracker.lookup(cookie);
            if (self != nullptr)
            {
                std::shared_ptr<SessionUpdatedEventArgs> args_eventArg = args ? std::make_shared<SessionUpdatedEventArgs>(args, false) : nullptr;
                self->m_sessionUpdatedEvent(self.get(), args_eventArg);
            }
        }));
        return token;
    }

    void CloudSpatialAnchorSession::SessionUpdated(event_token const& token)
    {
        m_sessionUpdatedEvent.remove(token);
    }

    event_token CloudSpatialAnchorSession::Error(SessionErrorDelegate const& handler)
    {
        auto token = m_errorEvent.add(handler);

        auto cookie = s_cloudSpatialAnchorSessionTracker.getCookie(shared_from_this());
        check_status(m_handle, ssc_cloud_spatial_anchor_session_set_error(static_cast<ssc_cloud_spatial_anchor_session_handle>(m_handle), cookie, [](ssc_callback_cookie cookie, ssc_session_error_event_args_handle args)
        {
            auto self = s_cloudSpatialAnchorSessionTracker.lookup(cookie);
            if (self != nullptr)
            {
                std::shared_ptr<SessionErrorEventArgs> args_eventArg = args ? std::make_shared<SessionErrorEventArgs>(args, false) : nullptr;
                self->m_errorEvent(self.get(), args_eventArg);
            }
        }));
        return token;
    }

    void CloudSpatialAnchorSession::Error(event_token const& token)
    {
        m_errorEvent.remove(token);
    }

    event_token CloudSpatialAnchorSession::OnLogDebug(OnLogDebugDelegate const& handler)
    {
        auto token = m_onLogDebugEvent.add(handler);

        auto cookie = s_cloudSpatialAnchorSessionTracker.getCookie(shared_from_this());
        check_status(m_handle, ssc_cloud_spatial_anchor_session_set_on_log_debug(static_cast<ssc_cloud_spatial_anchor_session_handle>(m_handle), cookie, [](ssc_callback_cookie cookie, ssc_on_log_debug_event_args_handle args)
        {
            auto self = s_cloudSpatialAnchorSessionTracker.lookup(cookie);
            if (self != nullptr)
            {
                std::shared_ptr<OnLogDebugEventArgs> args_eventArg = args ? std::make_shared<OnLogDebugEventArgs>(args, false) : nullptr;
                self->m_onLogDebugEvent(self.get(), args_eventArg);
            }
        }));
        return token;
    }

    void CloudSpatialAnchorSession::OnLogDebug(event_token const& token)
    {
        m_onLogDebugEvent.remove(token);
    }

    void CloudSpatialAnchorSession::Dispose()
    {
        check_status(m_handle, ssc_cloud_spatial_anchor_session_dispose(static_cast<ssc_cloud_spatial_anchor_session_handle>(m_handle)));
    }

    void CloudSpatialAnchorSession::GetAccessTokenWithAuthenticationTokenAsync(std::string const & param_authenticationToken, std::function<void(Status, const std::string &)> callback)
    {
        auto self = shared_from_this();
        std::string authenticationToken = param_authenticationToken;
        thread_pool::get().submit_work([self, callback, authenticationToken]
        {
            const char * outParam_result;
            auto status = (ssc_cloud_spatial_anchor_session_get_access_token_with_authentication_token_async(static_cast<ssc_cloud_spatial_anchor_session_handle>(self->m_handle), authenticationToken.c_str(), &outParam_result));
            std::string result = outParam_result; CoTaskMemFree(const_cast<char*>(outParam_result));
            callback(static_cast<Status>(status), result);
        });
    }

    void CloudSpatialAnchorSession::GetAccessTokenWithAccountKeyAsync(std::string const & param_accountKey, std::function<void(Status, const std::string &)> callback)
    {
        auto self = shared_from_this();
        std::string accountKey = param_accountKey;
        thread_pool::get().submit_work([self, callback, accountKey]
        {
            const char * outParam_result;
            auto status = (ssc_cloud_spatial_anchor_session_get_access_token_with_account_key_async(static_cast<ssc_cloud_spatial_anchor_session_handle>(self->m_handle), accountKey.c_str(), &outParam_result));
            std::string result = outParam_result; CoTaskMemFree(const_cast<char*>(outParam_result));
            callback(static_cast<Status>(status), result);
        });
    }

    void CloudSpatialAnchorSession::CreateAnchorAsync(std::shared_ptr<CloudSpatialAnchor> const & param_anchor, std::function<void(Status)> callback)
    {
        auto self = shared_from_this();
        std::shared_ptr<CloudSpatialAnchor> anchor = param_anchor;
        thread_pool::get().submit_work([self, callback, anchor]
        {
            auto status = (ssc_cloud_spatial_anchor_session_create_anchor_async(static_cast<ssc_cloud_spatial_anchor_session_handle>(self->m_handle), static_cast<ssc_cloud_spatial_anchor_handle>(anchor->Handle())));
            callback(static_cast<Status>(status));
        });
    }

    std::shared_ptr<CloudSpatialAnchorWatcher> CloudSpatialAnchorSession::CreateWatcher(std::shared_ptr<AnchorLocateCriteria> const & criteria)
    {
        ssc_cloud_spatial_anchor_watcher_handle outParam_result;
        check_status(m_handle, ssc_cloud_spatial_anchor_session_create_watcher(static_cast<ssc_cloud_spatial_anchor_session_handle>(m_handle), static_cast<ssc_anchor_locate_criteria_handle>(criteria->Handle()), &outParam_result));
        std::shared_ptr<CloudSpatialAnchorWatcher> result = outParam_result ? std::make_shared<CloudSpatialAnchorWatcher>(outParam_result) : nullptr;
        return result;
    }

    void CloudSpatialAnchorSession::GetAnchorPropertiesAsync(std::string const & param_identifier, std::function<void(Status, const std::shared_ptr<CloudSpatialAnchor> &)> callback)
    {
        auto self = shared_from_this();
        std::string identifier = param_identifier;
        thread_pool::get().submit_work([self, callback, identifier]
        {
            ssc_cloud_spatial_anchor_handle outParam_result;
            auto status = (ssc_cloud_spatial_anchor_session_get_anchor_properties_async(static_cast<ssc_cloud_spatial_anchor_session_handle>(self->m_handle), identifier.c_str(), &outParam_result));
            std::shared_ptr<CloudSpatialAnchor> result = outParam_result ? std::make_shared<CloudSpatialAnchor>(outParam_result) : nullptr;
            callback(static_cast<Status>(status), result);
        });
    }

    std::vector<std::shared_ptr<CloudSpatialAnchorWatcher>> CloudSpatialAnchorSession::GetActiveWatchers()
    {
        ssc_cloud_spatial_anchor_watcher_handle * outParam_result;
        int result_length = 0;
        check_status(m_handle, ssc_cloud_spatial_anchor_session_get_active_watchers(static_cast<ssc_cloud_spatial_anchor_session_handle>(m_handle), &outParam_result, &result_length));
        auto result = create_vector_CloudSpatialAnchorWatcher(outParam_result, result_length); CoTaskMemFree(outParam_result);
        return result;
    }

    void CloudSpatialAnchorSession::RefreshAnchorPropertiesAsync(std::shared_ptr<CloudSpatialAnchor> const & param_anchor, std::function<void(Status)> callback)
    {
        auto self = shared_from_this();
        std::shared_ptr<CloudSpatialAnchor> anchor = param_anchor;
        thread_pool::get().submit_work([self, callback, anchor]
        {
            auto status = (ssc_cloud_spatial_anchor_session_refresh_anchor_properties_async(static_cast<ssc_cloud_spatial_anchor_session_handle>(self->m_handle), static_cast<ssc_cloud_spatial_anchor_handle>(anchor->Handle())));
            callback(static_cast<Status>(status));
        });
    }

    void CloudSpatialAnchorSession::UpdateAnchorPropertiesAsync(std::shared_ptr<CloudSpatialAnchor> const & param_anchor, std::function<void(Status)> callback)
    {
        auto self = shared_from_this();
        std::shared_ptr<CloudSpatialAnchor> anchor = param_anchor;
        thread_pool::get().submit_work([self, callback, anchor]
        {
            auto status = (ssc_cloud_spatial_anchor_session_update_anchor_properties_async(static_cast<ssc_cloud_spatial_anchor_session_handle>(self->m_handle), static_cast<ssc_cloud_spatial_anchor_handle>(anchor->Handle())));
            callback(static_cast<Status>(status));
        });
    }

    void CloudSpatialAnchorSession::DeleteAnchorAsync(std::shared_ptr<CloudSpatialAnchor> const & param_anchor, std::function<void(Status)> callback)
    {
        auto self = shared_from_this();
        std::shared_ptr<CloudSpatialAnchor> anchor = param_anchor;
        thread_pool::get().submit_work([self, callback, anchor]
        {
            auto status = (ssc_cloud_spatial_anchor_session_delete_anchor_async(static_cast<ssc_cloud_spatial_anchor_session_handle>(self->m_handle), static_cast<ssc_cloud_spatial_anchor_handle>(anchor->Handle())));
            callback(static_cast<Status>(status));
        });
    }

    void CloudSpatialAnchorSession::GetSessionStatusAsync(std::function<void(Status, const std::shared_ptr<SessionStatus> &)> callback)
    {
        auto self = shared_from_this();
        thread_pool::get().submit_work([self, callback]
        {
            ssc_session_status_handle outParam_result;
            auto status = (ssc_cloud_spatial_anchor_session_get_session_status_async(static_cast<ssc_cloud_spatial_anchor_session_handle>(self->m_handle), &outParam_result));
            std::shared_ptr<SessionStatus> result = outParam_result ? std::make_shared<SessionStatus>(outParam_result) : nullptr;
            callback(static_cast<Status>(status), result);
        });
    }

    void CloudSpatialAnchorSession::Start()
    {
        check_status(m_handle, ssc_cloud_spatial_anchor_session_start(static_cast<ssc_cloud_spatial_anchor_session_handle>(m_handle)));
    }

    void CloudSpatialAnchorSession::Stop()
    {
        check_status(m_handle, ssc_cloud_spatial_anchor_session_stop(static_cast<ssc_cloud_spatial_anchor_session_handle>(m_handle)));
    }

    void CloudSpatialAnchorSession::Reset()
    {
        check_status(m_handle, ssc_cloud_spatial_anchor_session_reset(static_cast<ssc_cloud_spatial_anchor_session_handle>(m_handle)));
    }

    CloudSpatialAnchorWatcher::CloudSpatialAnchorWatcher(void* handle, bool noCopy)
        : m_handle(handle)
    {
        if (!noCopy)
        {
            check_status(m_handle, ssc_cloud_spatial_anchor_watcher_addref(static_cast<ssc_cloud_spatial_anchor_watcher_handle>(m_handle)));
        }
    }

    CloudSpatialAnchorWatcher::~CloudSpatialAnchorWatcher()
    {
        check_status(m_handle, ssc_cloud_spatial_anchor_watcher_release(static_cast<ssc_cloud_spatial_anchor_watcher_handle>(m_handle)));
    }

    void* CloudSpatialAnchorWatcher::Handle() const
    {
        return m_handle;
    }

    int32_t CloudSpatialAnchorWatcher::Identifier()
    {
        int outParam_result;
        check_status(m_handle, ssc_cloud_spatial_anchor_watcher_get_identifier(static_cast<ssc_cloud_spatial_anchor_watcher_handle>(m_handle), &outParam_result));
        int32_t result = outParam_result;
        return result;
    }

    void CloudSpatialAnchorWatcher::Stop()
    {
        check_status(m_handle, ssc_cloud_spatial_anchor_watcher_stop(static_cast<ssc_cloud_spatial_anchor_watcher_handle>(m_handle)));
    }

    LocateAnchorsCompletedEventArgs::LocateAnchorsCompletedEventArgs(void* handle, bool noCopy)
        : m_handle(handle)
    {
        if (!noCopy)
        {
            check_status(m_handle, ssc_locate_anchors_completed_event_args_addref(static_cast<ssc_locate_anchors_completed_event_args_handle>(m_handle)));
        }
    }

    LocateAnchorsCompletedEventArgs::~LocateAnchorsCompletedEventArgs()
    {
        check_status(m_handle, ssc_locate_anchors_completed_event_args_release(static_cast<ssc_locate_anchors_completed_event_args_handle>(m_handle)));
    }

    void* LocateAnchorsCompletedEventArgs::Handle() const
    {
        return m_handle;
    }

    bool LocateAnchorsCompletedEventArgs::Cancelled()
    {
        char outParam_result;
        check_status(m_handle, ssc_locate_anchors_completed_event_args_get_cancelled(static_cast<ssc_locate_anchors_completed_event_args_handle>(m_handle), &outParam_result));
        bool result = static_cast<bool>(outParam_result);
        return result;
    }

    std::shared_ptr<CloudSpatialAnchorWatcher> LocateAnchorsCompletedEventArgs::Watcher()
    {
        ssc_cloud_spatial_anchor_watcher_handle outParam_result;
        check_status(m_handle, ssc_locate_anchors_completed_event_args_get_watcher(static_cast<ssc_locate_anchors_completed_event_args_handle>(m_handle), &outParam_result));
        std::shared_ptr<CloudSpatialAnchorWatcher> result = outParam_result ? std::make_shared<CloudSpatialAnchorWatcher>(outParam_result) : nullptr;
        return result;
    }

    NearAnchorCriteria::NearAnchorCriteria(void* handle, bool noCopy)
        : m_handle(handle)
    {
        if (!noCopy)
        {
            check_status(m_handle, ssc_near_anchor_criteria_addref(static_cast<ssc_near_anchor_criteria_handle>(m_handle)));
        }
    }

    NearAnchorCriteria::~NearAnchorCriteria()
    {
        check_status(m_handle, ssc_near_anchor_criteria_release(static_cast<ssc_near_anchor_criteria_handle>(m_handle)));
    }

    void* NearAnchorCriteria::Handle() const
    {
        return m_handle;
    }

    NearAnchorCriteria::NearAnchorCriteria()
    {
        ssc_status resultStatus = ssc_near_anchor_criteria_create(reinterpret_cast<ssc_near_anchor_criteria_handle*>(&m_handle));
        check_status(m_handle, resultStatus);
    }

    std::shared_ptr<CloudSpatialAnchor> NearAnchorCriteria::SourceAnchor()
    {
        ssc_cloud_spatial_anchor_handle outParam_result;
        check_status(m_handle, ssc_near_anchor_criteria_get_source_anchor(static_cast<ssc_near_anchor_criteria_handle>(m_handle), &outParam_result));
        std::shared_ptr<CloudSpatialAnchor> result = outParam_result ? std::make_shared<CloudSpatialAnchor>(outParam_result) : nullptr;
        return result;
    }

    void NearAnchorCriteria::SourceAnchor(std::shared_ptr<CloudSpatialAnchor> const& value)
    {
        check_status(m_handle, ssc_near_anchor_criteria_set_source_anchor(static_cast<ssc_near_anchor_criteria_handle>(m_handle), static_cast<ssc_cloud_spatial_anchor_handle>(value->Handle())));
    }

    float NearAnchorCriteria::DistanceInMeters()
    {
        float outParam_result;
        check_status(m_handle, ssc_near_anchor_criteria_get_distance_in_meters(static_cast<ssc_near_anchor_criteria_handle>(m_handle), &outParam_result));
        float result = outParam_result;
        return result;
    }

    void NearAnchorCriteria::DistanceInMeters(float const& value)
    {
        check_status(m_handle, ssc_near_anchor_criteria_set_distance_in_meters(static_cast<ssc_near_anchor_criteria_handle>(m_handle), static_cast<float>(value)));
    }

    int32_t NearAnchorCriteria::MaxResultCount()
    {
        int outParam_result;
        check_status(m_handle, ssc_near_anchor_criteria_get_max_result_count(static_cast<ssc_near_anchor_criteria_handle>(m_handle), &outParam_result));
        int32_t result = outParam_result;
        return result;
    }

    void NearAnchorCriteria::MaxResultCount(int32_t const& value)
    {
        check_status(m_handle, ssc_near_anchor_criteria_set_max_result_count(static_cast<ssc_near_anchor_criteria_handle>(m_handle), static_cast<int32_t>(value)));
    }

    OnLogDebugEventArgs::OnLogDebugEventArgs(void* handle, bool noCopy)
        : m_handle(handle)
    {
        if (!noCopy)
        {
            check_status(m_handle, ssc_on_log_debug_event_args_addref(static_cast<ssc_on_log_debug_event_args_handle>(m_handle)));
        }
    }

    OnLogDebugEventArgs::~OnLogDebugEventArgs()
    {
        check_status(m_handle, ssc_on_log_debug_event_args_release(static_cast<ssc_on_log_debug_event_args_handle>(m_handle)));
    }

    void* OnLogDebugEventArgs::Handle() const
    {
        return m_handle;
    }

    std::string OnLogDebugEventArgs::Message()
    {
        const char * outParam_result;
        check_status(m_handle, ssc_on_log_debug_event_args_get_message(static_cast<ssc_on_log_debug_event_args_handle>(m_handle), &outParam_result));
        std::string result = outParam_result; CoTaskMemFree(const_cast<char*>(outParam_result));
        return result;
    }

    SessionConfiguration::SessionConfiguration(void* handle, bool noCopy)
        : m_handle(handle)
    {
        if (!noCopy)
        {
            check_status(m_handle, ssc_session_configuration_addref(static_cast<ssc_session_configuration_handle>(m_handle)));
        }
    }

    SessionConfiguration::~SessionConfiguration()
    {
        check_status(m_handle, ssc_session_configuration_release(static_cast<ssc_session_configuration_handle>(m_handle)));
    }

    void* SessionConfiguration::Handle() const
    {
        return m_handle;
    }

    std::string SessionConfiguration::AccountDomain()
    {
        const char * outParam_result;
        check_status(m_handle, ssc_session_configuration_get_account_domain(static_cast<ssc_session_configuration_handle>(m_handle), &outParam_result));
        std::string result = outParam_result; CoTaskMemFree(const_cast<char*>(outParam_result));
        return result;
    }

    void SessionConfiguration::AccountDomain(std::string const& value)
    {
        check_status(m_handle, ssc_session_configuration_set_account_domain(static_cast<ssc_session_configuration_handle>(m_handle), value.c_str()));
    }

    std::string SessionConfiguration::AccountId()
    {
        const char * outParam_result;
        check_status(m_handle, ssc_session_configuration_get_account_id(static_cast<ssc_session_configuration_handle>(m_handle), &outParam_result));
        std::string result = outParam_result; CoTaskMemFree(const_cast<char*>(outParam_result));
        return result;
    }

    void SessionConfiguration::AccountId(std::string const& value)
    {
        check_status(m_handle, ssc_session_configuration_set_account_id(static_cast<ssc_session_configuration_handle>(m_handle), value.c_str()));
    }

    std::string SessionConfiguration::AuthenticationToken()
    {
        const char * outParam_result;
        check_status(m_handle, ssc_session_configuration_get_authentication_token(static_cast<ssc_session_configuration_handle>(m_handle), &outParam_result));
        std::string result = outParam_result; CoTaskMemFree(const_cast<char*>(outParam_result));
        return result;
    }

    void SessionConfiguration::AuthenticationToken(std::string const& value)
    {
        check_status(m_handle, ssc_session_configuration_set_authentication_token(static_cast<ssc_session_configuration_handle>(m_handle), value.c_str()));
    }

    std::string SessionConfiguration::AccountKey()
    {
        const char * outParam_result;
        check_status(m_handle, ssc_session_configuration_get_account_key(static_cast<ssc_session_configuration_handle>(m_handle), &outParam_result));
        std::string result = outParam_result; CoTaskMemFree(const_cast<char*>(outParam_result));
        return result;
    }

    void SessionConfiguration::AccountKey(std::string const& value)
    {
        check_status(m_handle, ssc_session_configuration_set_account_key(static_cast<ssc_session_configuration_handle>(m_handle), value.c_str()));
    }

    std::string SessionConfiguration::AccessToken()
    {
        const char * outParam_result;
        check_status(m_handle, ssc_session_configuration_get_access_token(static_cast<ssc_session_configuration_handle>(m_handle), &outParam_result));
        std::string result = outParam_result; CoTaskMemFree(const_cast<char*>(outParam_result));
        return result;
    }

    void SessionConfiguration::AccessToken(std::string const& value)
    {
        check_status(m_handle, ssc_session_configuration_set_access_token(static_cast<ssc_session_configuration_handle>(m_handle), value.c_str()));
    }

    SessionErrorEventArgs::SessionErrorEventArgs(void* handle, bool noCopy)
        : m_handle(handle)
    {
        if (!noCopy)
        {
            check_status(m_handle, ssc_session_error_event_args_addref(static_cast<ssc_session_error_event_args_handle>(m_handle)));
        }
    }

    SessionErrorEventArgs::~SessionErrorEventArgs()
    {
        check_status(m_handle, ssc_session_error_event_args_release(static_cast<ssc_session_error_event_args_handle>(m_handle)));
    }

    void* SessionErrorEventArgs::Handle() const
    {
        return m_handle;
    }

    CloudSpatialErrorCode SessionErrorEventArgs::ErrorCode()
    {
        ssc_cloud_spatial_error_code outParam_result;
        check_status(m_handle, ssc_session_error_event_args_get_error_code(static_cast<ssc_session_error_event_args_handle>(m_handle), &outParam_result));
        auto result = static_cast<CloudSpatialErrorCode>(outParam_result);
        return result;
    }

    std::string SessionErrorEventArgs::ErrorMessage()
    {
        const char * outParam_result;
        check_status(m_handle, ssc_session_error_event_args_get_error_message(static_cast<ssc_session_error_event_args_handle>(m_handle), &outParam_result));
        std::string result = outParam_result; CoTaskMemFree(const_cast<char*>(outParam_result));
        return result;
    }

    std::shared_ptr<CloudSpatialAnchorWatcher> SessionErrorEventArgs::Watcher()
    {
        ssc_cloud_spatial_anchor_watcher_handle outParam_result;
        check_status(m_handle, ssc_session_error_event_args_get_watcher(static_cast<ssc_session_error_event_args_handle>(m_handle), &outParam_result));
        std::shared_ptr<CloudSpatialAnchorWatcher> result = outParam_result ? std::make_shared<CloudSpatialAnchorWatcher>(outParam_result) : nullptr;
        return result;
    }

    SessionStatus::SessionStatus(void* handle, bool noCopy)
        : m_handle(handle)
    {
        if (!noCopy)
        {
            check_status(m_handle, ssc_session_status_addref(static_cast<ssc_session_status_handle>(m_handle)));
        }
    }

    SessionStatus::~SessionStatus()
    {
        check_status(m_handle, ssc_session_status_release(static_cast<ssc_session_status_handle>(m_handle)));
    }

    void* SessionStatus::Handle() const
    {
        return m_handle;
    }

    float SessionStatus::ReadyForCreateProgress()
    {
        float outParam_result;
        check_status(m_handle, ssc_session_status_get_ready_for_create_progress(static_cast<ssc_session_status_handle>(m_handle), &outParam_result));
        float result = outParam_result;
        return result;
    }

    float SessionStatus::RecommendedForCreateProgress()
    {
        float outParam_result;
        check_status(m_handle, ssc_session_status_get_recommended_for_create_progress(static_cast<ssc_session_status_handle>(m_handle), &outParam_result));
        float result = outParam_result;
        return result;
    }

    int32_t SessionStatus::SessionCreateHash()
    {
        int outParam_result;
        check_status(m_handle, ssc_session_status_get_session_create_hash(static_cast<ssc_session_status_handle>(m_handle), &outParam_result));
        int32_t result = outParam_result;
        return result;
    }

    int32_t SessionStatus::SessionLocateHash()
    {
        int outParam_result;
        check_status(m_handle, ssc_session_status_get_session_locate_hash(static_cast<ssc_session_status_handle>(m_handle), &outParam_result));
        int32_t result = outParam_result;
        return result;
    }

    SessionUserFeedback SessionStatus::UserFeedback()
    {
        ssc_session_user_feedback outParam_result;
        check_status(m_handle, ssc_session_status_get_user_feedback(static_cast<ssc_session_status_handle>(m_handle), &outParam_result));
        auto result = static_cast<SessionUserFeedback>(outParam_result);
        return result;
    }

    SessionUpdatedEventArgs::SessionUpdatedEventArgs(void* handle, bool noCopy)
        : m_handle(handle)
    {
        if (!noCopy)
        {
            check_status(m_handle, ssc_session_updated_event_args_addref(static_cast<ssc_session_updated_event_args_handle>(m_handle)));
        }
    }

    SessionUpdatedEventArgs::~SessionUpdatedEventArgs()
    {
        check_status(m_handle, ssc_session_updated_event_args_release(static_cast<ssc_session_updated_event_args_handle>(m_handle)));
    }

    void* SessionUpdatedEventArgs::Handle() const
    {
        return m_handle;
    }

    std::shared_ptr<SessionStatus> SessionUpdatedEventArgs::Status()
    {
        ssc_session_status_handle outParam_result;
        check_status(m_handle, ssc_session_updated_event_args_get_status(static_cast<ssc_session_updated_event_args_handle>(m_handle), &outParam_result));
        std::shared_ptr<SessionStatus> result = outParam_result ? std::make_shared<SessionStatus>(outParam_result) : nullptr;
        return result;
    }

    TokenRequiredEventArgs::TokenRequiredEventArgs(void* handle, bool noCopy)
        : m_handle(handle)
    {
        if (!noCopy)
        {
            check_status(m_handle, ssc_token_required_event_args_addref(static_cast<ssc_token_required_event_args_handle>(m_handle)));
        }
    }

    TokenRequiredEventArgs::~TokenRequiredEventArgs()
    {
        check_status(m_handle, ssc_token_required_event_args_release(static_cast<ssc_token_required_event_args_handle>(m_handle)));
    }

    void* TokenRequiredEventArgs::Handle() const
    {
        return m_handle;
    }

    std::string TokenRequiredEventArgs::AccessToken()
    {
        const char * outParam_result;
        check_status(m_handle, ssc_token_required_event_args_get_access_token(static_cast<ssc_token_required_event_args_handle>(m_handle), &outParam_result));
        std::string result = outParam_result; CoTaskMemFree(const_cast<char*>(outParam_result));
        return result;
    }

    void TokenRequiredEventArgs::AccessToken(std::string const& value)
    {
        check_status(m_handle, ssc_token_required_event_args_set_access_token(static_cast<ssc_token_required_event_args_handle>(m_handle), value.c_str()));
    }

    std::string TokenRequiredEventArgs::AuthenticationToken()
    {
        const char * outParam_result;
        check_status(m_handle, ssc_token_required_event_args_get_authentication_token(static_cast<ssc_token_required_event_args_handle>(m_handle), &outParam_result));
        std::string result = outParam_result; CoTaskMemFree(const_cast<char*>(outParam_result));
        return result;
    }

    void TokenRequiredEventArgs::AuthenticationToken(std::string const& value)
    {
        check_status(m_handle, ssc_token_required_event_args_set_authentication_token(static_cast<ssc_token_required_event_args_handle>(m_handle), value.c_str()));
    }

    std::shared_ptr<CloudSpatialAnchorSessionDeferral> TokenRequiredEventArgs::GetDeferral()
    {
        ssc_cloud_spatial_anchor_session_deferral_handle outParam_result;
        check_status(m_handle, ssc_token_required_event_args_get_deferral(static_cast<ssc_token_required_event_args_handle>(m_handle), &outParam_result));
        std::shared_ptr<CloudSpatialAnchorSessionDeferral> result = outParam_result ? std::make_shared<CloudSpatialAnchorSessionDeferral>(outParam_result) : nullptr;
        return result;
    }

} } } 

