using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DigitalPlatform.LibraryRestClient
{

    #region dp2Kernel数据结构

    [DataContract(Namespace = "http://dp2003.com/dp2kernel/")]
    public enum ErrorCodeValue
    {
        [EnumMember]
        NoError = 0,	 // 没有错误
        [EnumMember]
        CommonError = 1, // 一般性错误   -1

        [EnumMember]
        NotLogin = 2,	// 尚未登录 (Dir/ListTask)
        [EnumMember]
        UserNameEmpty = 3,	// 用户名为空 (Login)
        [EnumMember]
        UserNameOrPasswordMismatch = 4,	// 用户名或者密码错误 (Login)

        //NoHasList = 5,     //没有列目录权限
        //NoHasRead = 6,     //没有读权限          
        //NoHasWrite = 7,    //没有写权限
        //NoHasManagement = 8, //没有管理员权限

        [EnumMember]
        NotHasEnoughRights = 5, // 没有足够的权限 -6

        [EnumMember]
        TimestampMismatch = 9,  //时间戳不匹配   -2
        [EnumMember]
        NotFound = 10, //没找到记录       -4
        [EnumMember]
        EmptyContent = 11,   //空记录  -3

        [EnumMember]
        NotFoundDb = 12,  // 没找到数据库 -5
        //OutOfRange = 13, // 范围越界
        [EnumMember]
        PathError = 14, // 路径不合法  -7

        [EnumMember]
        PartNotFound = 15, // 通过xpath未找到节点 -10

        [EnumMember]
        ExistDbInfo = 16,  //在新建库中，发现已经存在相同的信息 -11

        [EnumMember]
        AlreadyExist = 17,	//已经存在	-8
        [EnumMember]
        AlreadyExistOtherType = 18,		// 存在不同类型的项 -9

        [EnumMember]
        ApplicationStartError = 19,	//Application启动错误

        [EnumMember]
        NotFoundSubRes = 20,    // 部分下级资源记录不存在

        [EnumMember]
        Canceled = 21,    // 操作被放弃 2011/1/19

        [EnumMember]
        AccessDenied = 22,  // 权限不够 2011/2/11
    };

    #endregion

    #region dp2Library数据结构
    // dp2Library API错误码
    public enum ErrorCode
    {
        NoError = 0,
        SystemError = 1,    // 系统错误。指application启动时的严重错误。
        NotFound = 2,   // 没有找到
        ReaderBarcodeNotFound = 3,  // 读者证条码号不存在
        ItemBarcodeNotFound = 4,  // 册条码号不存在
        Overdue = 5,    // 还书过程发现有超期情况（已经按还书处理完毕，并且已经将超期信息记载到读者记录中，但是需要提醒读者及时履行超期违约金等手续）
        NotLogin = 6,   // 尚未登录
        DupItemBarcode = 7, // 预约中本次提交的某些册条码号被本读者先前曾预约过
        InvalidParameter = 8,   // 不合法的参数
        ReturnReservation = 9,    // 还书操作成功, 因属于被预约图书, 请放入预约保留架
        BorrowReservationDenied = 10,    // 借书操作失败, 因属于被预约(到书)保留的图书, 非当前预约者不能借阅
        RenewReservationDenied = 11,    // 续借操作失败, 因属于被预约的图书
        AccessDenied = 12,  // 存取被拒绝
        ChangePartDenied = 13,    // 部分修改被拒绝
        ItemBarcodeDup = 14,    // 册条码号重复
        Hangup = 15,    // 系统挂起
        ReaderBarcodeDup = 16,  // 读者证条码号重复
        HasCirculationInfo = 17,    // 包含流通信息(不能删除)
        SourceReaderBarcodeNotFound = 18,  // 源读者证条码号不存在
        TargetReaderBarcodeNotFound = 19,  // 目标读者证条码号不存在
        FromNotFound = 20,  // 检索途径(from caption或者style)没有找到
        ItemDbNotDef = 21,  // 实体库没有定义
        IdcardNumberDup = 22,   // 身份证号检索点命中读者记录不唯一。因为无法用它借书还书。但是可以用证条码号来进行
        IdcardNumberNotFound = 23,  // 身份证号不存在

        // 以下为兼容内核错误码而设立的同名错误码
        AlreadyExist = 100, // 兼容
        AlreadyExistOtherType = 101,
        ApplicationStartError = 102,
        EmptyRecord = 103,
        // None = 104, 采用了NoError
        NotFoundSubRes = 105,
        NotHasEnoughRights = 106,
        OtherError = 107,
        PartNotFound = 108,
        RequestCanceled = 109,
        RequestCanceledByEventClose = 110,
        RequestError = 111,
        RequestTimeOut = 112,
        TimestampMismatch = 113,
    }

    // API函数结果
    [DataContract]
    public class LibraryServerResult
    {
        [DataMember]
        public long Value { get; set; }
        [DataMember]
        public string ErrorInfo { get; set; }
        [DataMember]
        public ErrorCode ErrorCode { get; set; }
    }

    #endregion

    #region API 参数 打包解包

    // 校验条码号
    // parameters:
    //      strLibraryCode  分馆代码
    //      strBarcode 条码号
    // return:
    //      result.Value 0: 不是合法的条码号 1:合法的读者证条码号 2:合法的册条码号
    // 权限：暂时不需要任何权限
    [DataContract]
    public class VerifyBarcodeRequest
    {
        [DataMember]
        public string strLibraryCode { get; set; }
        [DataMember]
        public string strBarcode { get; set; }
    }

    [DataContract]
    public class VerifyBarcodeResponse
    {
        [DataMember]
        public LibraryServerResult VerifyBarcodeResult { get; set; }

    }


    // SearchBiblioRequest
    [DataContract]
    public class SearchBiblioRequest
    {
        [DataMember]
        public string strBiblioDbNames { get; set; }
        [DataMember]
        public string strQueryWord { get; set; }
        [DataMember]
        public int nPerMax { get; set; }
        [DataMember]
        public string strFromStyle { get; set; }
        [DataMember]
        public string strMatchStyle { get; set; }
        [DataMember]
        public string strLang { get; set; }
        [DataMember]
        public string strResultSetName { get; set; }
        [DataMember]
        public string strSearchStyle { get; set; }
        [DataMember]
        public string strOutputStyle { get; set; }
    }

    //SearchBiblioResult
    [DataContract]
    public class SearchBiblioResponse
    {
        [DataMember]
        public LibraryServerResult SearchBiblioResult { get; set; }

        // strQueryXml
        [DataMember]
        public string strQueryXml { get; set; }
    }

    // GetVersion()
    [DataContract]
    public class GetVersionResponse
    {
        [DataMember]
        public LibraryServerResult GetVersionResult { get; set; }
    }

    // Login()
    [DataContract]
    public class LoginRequest
    {
        [DataMember]
        public string strUserName { get; set; }
        [DataMember]
        public string strPassword { get; set; }
        [DataMember]
        public string strParameters { get; set; }
    }

    [DataContract]
    public class LoginResponse
    {
        [DataMember]
        public LibraryServerResult LoginResult { get; set; }
        [DataMember]
        public string strOutputUserName { get; set; }
        [DataMember]
        public string strRights { get; set; }

        [DataMember]
        public string strLibraryCode { get; set; }
        
    }

    // Logout()
    [DataContract]
    public class LogoutResponse
    {
        [DataMember]
        public LibraryServerResult LogoutResult { get; set; }
    }

    // SetLang()
    [DataContract]
    public class SetLangRequest
    {
        [DataMember]
        public string strLang { get; set; }
    }

    [DataContract]
    public class SetLangResponse
    {
        [DataMember]
        public LibraryServerResult SetLangResult { get; set; }
        [DataMember]
        public string strOldLang { get; set; }
    }

    // GetReaderInfo
    [DataContract]
    public class GetReaderInfoRequest
    {
        [DataMember]
        public string strBarcode { get; set; }
        [DataMember]
        public string strResultTypeList { get; set; }
    }

    [DataContract]
    public class GetReaderInfoResponse
    {
        [DataMember]
        public LibraryServerResult GetReaderInfoResult { get; set; }
        [DataMember]
        public string[] results { get; set; }
        [DataMember]
        public string strRecPath { get; set; }
        [DataMember]
        public byte[] baTimestamp { get; set; }
    }

    // SetReaderInfo
    [DataContract]
    public class SetReaderInfoRequest
    {
        [DataMember]
        public string strAction { get; set; }
        [DataMember]
        public string strRecPath { get; set; }
        [DataMember]
        public string strNewXml { get; set; }
        [DataMember]
        public string strOldXml { get; set; }
        [DataMember]
        public byte[] baOldTimestamp { get; set; }
    }

    [DataContract]
    public class SetReaderInfoResponse
    {
        [DataMember]
        public LibraryServerResult SetReaderInfoResult { get; set; }
        [DataMember]
        public string strExistingXml { get; set; }
        [DataMember]
        public string strSavedXml { get; set; }
        [DataMember]
        public string strSavedRecPath { get; set; }
        [DataMember]
        public byte[] baNewTimestamp { get; set; }
        [DataMember]
        public ErrorCodeValue kernel_errorcode { get; set; }
    }

    // VerifyReaderPassword
    [DataContract]
    public class VerifyReaderPasswordRequest
    {
        [DataMember]
        public string strReaderBarcode { get; set; }
        [DataMember]
        public string strReaderPassword { get; set; }
    }

    [DataContract]
    public class VerifyReaderPasswordResponse
    {
        [DataMember]
        public LibraryServerResult VerifyReaderPasswordResult { get; set; }
    }

    // ChangeReaderPassword
    [DataContract]
    public class ChangeReaderPasswordRequest
    {
        [DataMember]
        public string strReaderBarcode { get; set; }
        [DataMember]
        public string strReaderOldPassword { get; set; }
        [DataMember]
        public string strReaderNewPassword { get; set; }
    }

    [DataContract]
    public class ChangeReaderPasswordResponse
    {
        [DataMember]
        public LibraryServerResult ChangeReaderPasswordResult { get; set; }
    }

    // WriteRes()
    [DataContract]
    public class WriteResRequest
    {
        [DataMember]
        public string strResPath { get; set; }
        [DataMember]
        public string strRanges { get; set; }
        [DataMember]
        public long lTotalLength { get; set; }
        [DataMember]
        public byte[] baContent { get; set; }
        [DataMember]
        public string strMetadata { get; set; }
        [DataMember]
        public string strStyle { get; set; }
        [DataMember]
        public byte[] baInputTimestamp { get; set; }
    }

    [DataContract]
    public class WriteResResponse
    {
        [DataMember]
        public LibraryServerResult WriteResResult { get; set; }
        [DataMember]
        public string strOutputResPath { get; set; }
        [DataMember]
        public byte[] baOutputTimestamp { get; set; }
    }
    // GetSearchResult()
    [DataContract]
    public class GetSearchResultRequest
    {
        [DataMember]
        public string strResultSetName { get; set; }
        [DataMember]
        public long lStart { get; set; }
        [DataMember]
        public long lCount { get; set; }
        [DataMember]
        public string strBrowseInfoStyle { get; set; }
        [DataMember]
        public string strLang { get; set; }

    }

    [DataContract]
    public class GetSearchResultResponse
    {
        [DataMember]
        public LibraryServerResult GetSearchResultResult { get; set; }
        [DataMember]
        public Record[] searchresults { get; set; }
    }
    [DataContract(Namespace = "http://dp2003.com/dp2kernel/")]
    public class Record
    {
        [DataMember]
        public string Path = ""; // 带库名的全路径 原来叫ID 2010/5/17 changed
        [DataMember]
        public KeyFrom[] Keys = null; // 检索命中的key from字符串数组 
        [DataMember]
        public string[] Cols = null;

        [DataMember]
        public RecordBody RecordBody = null; // 记录体。2012/1/5
    }

    [DataContract(Namespace = "http://dp2003.com/dp2kernel/")]
    public class RecordBody
    {
        [DataMember]
        public string Xml = "";
        [DataMember]
        public byte[] Timestamp = null;
        [DataMember]
        public string Metadata = "";

        [DataMember]
        public Result Result = new Result(); // 结果信息
    }

    [DataContract(Namespace = "http://dp2003.com/dp2kernel/")]
    public class KeyFrom
    {
        [DataMember]
        public string Logic = "";
        [DataMember]
        public string Key = "";
        [DataMember]
        public string From = "";
    }

    [DataContract(Namespace = "http://dp2003.com/dp2kernel/")]
    public class Result
    {
        [DataMember]
        public long Value = 0; // 命中条数，>=0:正常;<0:出错

        [DataMember]
        public ErrorCodeValue ErrorCode = ErrorCodeValue.NoError;
        [DataMember]
        public string ErrorString = "错误信息未初始化...";
    }

    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class EntityInfo
    {
        [DataMember]
        public string RefID = "";  // 参考ID
        [DataMember]
        public string OldRecPath = "";  // 旧记录路径
        [DataMember]
        public string OldRecord = "";   // 旧记录
        [DataMember]
        public byte[] OldTimestamp = null;  // 旧记录的时间戳

        [DataMember]
        public string NewRecPath = ""; // 新记录路径
        [DataMember]
        public string NewRecord = "";   // 新记录
        [DataMember]
        public byte[] NewTimestamp = null;  // 新记录的时间戳

        [DataMember]
        public string Action = "";   // 要执行的操作(get时此项无用)
        [DataMember]
        public string Style = "";   // 附加的特性参数

        [DataMember]
        public string ErrorInfo = "";   // 出错信息
        [DataMember]
        public ErrorCodeValue ErrorCode = ErrorCodeValue.NoError;   // 错误码
    }

    //searchReader
    [DataContract]
    public class SearchReaderRequest
    {
        [DataMember]
        public string strReaderDbNames { get; set; }
        [DataMember]
        public string strQueryWord { get; set; }
        [DataMember]
        public int nPerMax { get; set; }
        [DataMember]
        public string strFrom { get; set; }
        [DataMember]
        public string strMatchStyle { get; set; }
        [DataMember]
        public string strLang { get; set; }
        [DataMember]
        public string strResultSetName { get; set; }
        [DataMember]
        public string strOutputStyle { get; set; }


    }
    [DataContract]
    public class SearchReaderResponse
    {
        [DataMember]
        public LibraryServerResult SearchReaderResult { get; set; }
    }

    //getItemInfo
    [DataContract]
    public class GetItemInfoRequest
    {
        [DataMember]
        public string strBarcode { get; set; }
        [DataMember]
        public string strResultType { get; set; }
        [DataMember]
        public string strBiblioType { get; set; }
    }
    [DataContract]
    public class GetItemInfoResponse
    {
        [DataMember]
        public LibraryServerResult GetItemInfoResult { get; set; }
        [DataMember]
        public string strResult { get; set; }
        [DataMember]
        public string strItemRecPath { get; set; }
        [DataMember]
        public byte[] baTimestamp { get; set; }
        [DataMember]
        public string strBiblio { get; set; }
        [DataMember]
        public string strBiblioRecPath { get; set; }
    }

    [DataContract]
    public class GetBiblioInfoRequest
    {
        [DataMember]
        public string strBiblioRecPath { get; set; }
        [DataMember]
        public string strBiblioXml { get; set; }
        [DataMember]
        public string strBiblioType { get; set; }
    }

    [DataContract]
    public class GetBiblioInfoResponse
    {
        [DataMember]
        public LibraryServerResult GetBiblioInfoResult { get; set; }
        [DataMember]
        public string strBiblio { get; set; }
    }

    [DataContract]
    public class GetEntitiesRequest
    {
        [DataMember]
        public string strBiblioRecPath { get; set; }
        [DataMember]
        public long lStart { get; set; }
        [DataMember]
        public long lCount { get; set; }
        [DataMember]
        public string strStyle { get; set; }
        [DataMember]
        public string strLang { get; set; }
    }

    [DataContract]
    public class GetEntitiesResponse
    {
        [DataMember]
        public LibraryServerResult GetEntitiesResult { get; set; }
        [DataMember]
        public EntityInfo[] entityinfos { get; set; }
    }


    [DataContract]
    public class GetBiblioSummaryRequest
    {
        [DataMember]
        public string strItemBarcode { get; set; }
        [DataMember]
        public string strConfirmItemRecPath { get; set; }
        [DataMember]
        public string strBiblioRecPathExclude { get; set; }
    }
    [DataContract]
    public class GetBiblioSummaryResponse
    {
        [DataMember]
        public LibraryServerResult GetBiblioSummaryResult { get; set; }
        [DataMember]
        public string strBiblioRecPath { get;set;}
        [DataMember]
        public string strSummary { get; set; }
    }

    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public enum MessageLevel
    {
        [EnumMember]
        ID = 0,    // 只返回ID
        [EnumMember]
        Summary = 1,    // 摘要级，不返回body
        [EnumMember]
        Full = 2,   // 全部级，返回全部信息
    }

    [DataContract]
    public class ListMessageRequest
    {
        [DataMember]
        public string strStyle { get; set; }
        [DataMember]
        public string strResultsetName { get; set; }
        [DataMember]
        public string strBoxType { get; set; }
        [DataMember]
        public MessageLevel messagelevel { get; set; }
        [DataMember]
        public int nStart { get; set; }
        [DataMember]
        public int nCount { get; set; }
    }
    [DataContract]
    public class ListMessageResponse
    {
        [DataMember]
        public LibraryServerResult ListMessageResult { get; set; }
        [DataMember]
        public int nTotalCount { get; set; }
        [DataMember]
        public List<MessageData> messages { get; set; }
    }

    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class MessageData
    {
        [DataMember]
        public string strUserName = ""; // 消息所从属的用户ID
        [DataMember]
        public string strBoxType = "";   // 信箱类型
        [DataMember]
        public string strSender = "";    // 发送者
        [DataMember]
        public string strRecipient = "";    // 接收者
        [DataMember]
        public string strSubject = "";  // 主题
        [DataMember]
        public string strMime = ""; // 媒体类型
        [DataMember]
        public string strBody = "";      // 正文内容
        [DataMember]
        public string strCreateTime = "";   // 邮件创建(收到)时间
        [DataMember]
        public string strSize = "";     // 尺寸
        [DataMember]
        public bool Touched = false;    // 是否阅读过
        [DataMember]
        public string strRecordID = ""; // 记录ID。用于唯一定位一条消息
        [DataMember]
        public byte[] TimeStamp = null;
    }


    [DataContract]
    public class GetOperLogRequest
    {
        [DataMember]
        public string strFileName { get; set; }
        [DataMember]
        public long lIndex { get; set; }
        [DataMember]
        public long lHint { get; set; }
        [DataMember]
        public long lAttachmentFragmentStart { get; set; }
        [DataMember]
        public int nAttachmentFragmentLength { get; set; }
    }
    [DataContract]
    public class GetOperLogResponse
    {
        [DataMember]
        public LibraryServerResult GetOperLogResult { get; set; }
        [DataMember]
        public string strXml { get; set; }
        [DataMember]
        public long lHintNext { get; set; }
        [DataMember]
        public byte[] attachment_data { get; set; }
        [DataMember]
        public long lAttachmentTotalLength { get; set; }
    }

   

    /*
        LibraryServerResult Borrow(
                    bool bRenew,
                    string strReaderBarcode,
                    string strItemBarcode,
                    string strConfirmItemRecPath,
                    bool bForce,

                    string[] saBorrowedItemBarcode,
                    string strStyle,
     
                    string strItemFormatList,
                    out string[] item_records,
                    string strReaderFormatList,
                    out string[] reader_records,
                    string strBiblioFormatList,
                    out string[] biblio_records,
                    out BorrowInfo borrow_info,
                    out string[] aDupPath,
                    out string strOutputReaderBarcode)
    */
    // WriteRes()
    [DataContract]
    public class BorrowRequest
    {
        [DataMember]
        public bool bRenew { get; set; }
        [DataMember]
        public string strReaderBarcode { get; set; }
        [DataMember]
        public string strItemBarcode { get; set; }
        [DataMember]
        public string strConfirmItemRecPath { get; set; }
        [DataMember]
        public bool bForce { get; set; }


        [DataMember]
        public string[] saBorrowedItemBarcode { get; set; }

        [DataMember]
        public string strStyle { get; set; }

        [DataMember]
        public string strItemFormatList { get; set; }

        [DataMember]
        public string strReaderFormatList { get; set; }

        [DataMember]
        public string strBiblioFormatList { get; set; }
    }

    [DataContract]
    public class BorrowResponse
    {
        [DataMember]
        public LibraryServerResult BorrowResult { get; set; }
        [DataMember]
        public string[] item_records { get; set; }
        [DataMember]
        public string[] reader_records { get; set; }

        [DataMember]
        public string[] biblio_records { get; set; }

        [DataMember]
        public BorrowInfo borrow_info { get; set; }

        [DataMember]
        public string[] aDupPath { get; set; }

        [DataMember]
        public string strOutputReaderBarcode { get; set; }
    }

    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class BorrowInfo
    {
        [DataMember]
        public string LatestReturnTime = "";  // 应还日期/时间
        [DataMember]
        public string Period = "";  //   // 借书期限。例如“20day”
        [DataMember]
        public long BorrowCount = 0;   // // 当前为续借的第几次？0表示初次借阅
        [DataMember]
        public string BorrowOperator = "";  //  借书操作者

    }


    //=======================
    /*
            LibraryServerResult Return(
                    string strAction,
                    string strReaderBarcode,
                    string strItemBarcode,
                    string strComfirmItemRecPath,
     * 
                    bool bForce,
                    string strStyle,
                    string strItemFormatList,
                    out string[] item_records,
     * 
                    string strReaderFormatList,
                    out string[] reader_records,
                    string strBiblioFormatList,
                    out string[] biblio_records,
                    out string[] aDupPath,
                    out string strOutputReaderBarcode,
                    out ReturnInfo return_info);
    */


    [DataContract]
    public class ReturnRequest
    {
        [DataMember]
        public string strAction { get; set; }
        [DataMember]
        public string strReaderBarcode { get; set; }
        [DataMember]
        public string strItemBarcode { get; set; }
        [DataMember]
        public string strConfirmItemRecPath { get; set; }
        
        //======
        [DataMember]
        public bool bForce { get; set; }
        [DataMember]
        public string strStyle { get; set; }

        [DataMember]
        public string strItemFormatList { get; set; }

        [DataMember]
        public string strReaderFormatList { get; set; }

        [DataMember]
        public string strBiblioFormatList { get; set; }
    }


    [DataContract]
    public class ReturnResponse
    {
        [DataMember]
        public LibraryServerResult ReturnResult { get; set; }

        [DataMember]
        public string[] item_records { get; set; }
        [DataMember]
        public string[] reader_records { get; set; }

        [DataMember]
        public string[] biblio_records { get; set; }

        [DataMember]
        public ReturnInfo return_info { get; set; }

        [DataMember]
        public string[] aDupPath { get; set; }

        [DataMember]
        public string strOutputReaderBarcode { get; set; }
    }

    // 还书成功后的信息
    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class ReturnInfo
    {
        // 借阅日期/时间
        [DataMember]
        public string BorrowTime = "";    // RFC1123格式，GMT时间

        // 应还日期/时间
        [DataMember]
        public string LatestReturnTime = "";    // RFC1123格式，GMT时间

        // 原借书期限。例如“20day”
        [DataMember]
        public string Period = "";

        // 当前为续借的第几次？0表示初次借阅
        [DataMember]
        public long BorrowCount = 0;

        // 违约金描述字符串。XML格式
        [DataMember]
        public string OverdueString = "";

        // 借书操作者
        [DataMember]
        public string BorrowOperator = "";

        // 还书操作者
        [DataMember]
        public string ReturnOperator = "";

        // 2008/5/9
        /// <summary>
        /// 所还的册的图书类型
        /// </summary>
        [DataMember]
        public string BookType = "";

        // 2008/5/9
        /// <summary>
        /// 所还的册的馆藏地点
        /// </summary>
        [DataMember]
        public string Location = "";
    }
    /*
            string[] paths,
            string strBrowseInfoStyle,
            out Record[] searchresults,
            out string strError)
     */
    public class GetBrowseRecordsRequest
    {
        [DataMember]
        public string[] paths { get; set; }

        [DataMember]
        public string strBrowseInfoStyle { get; set; }
    }

    [DataContract]
    public class GetBrowseRecordsResponse
    {
        [DataMember]
        public LibraryServerResult GetBrowseRecordsResult { get; set; }

        [DataMember]
        public Record[] searchresults { get; set; }
    }

    #endregion
}
