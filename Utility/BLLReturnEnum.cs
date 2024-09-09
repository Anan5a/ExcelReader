﻿namespace Utility
{
    public enum BLLReturnEnum
    {
        ACTION_OK,
        ACTION_ERROR,

        /// <summary>
        /// User related values
        /// </summary>
        User_USER_NOT_FOUND,
        User_USER_ALREADY_EXISTS,
        User_USER_AUTH_FAILED,
        User_USER_AUTH_SUCCESS,
        User_USER_ACCOUNT_ERROR,
        User_USER_ACCOUNT_DELETED,
        User_USER_NO_USER_CREATED,

        /// <summary>
        /// Admin related value
        /// </summary>
        Admin_ADMIN_NO_ROLE_LIST,
        Admin_ADMIN_CREATE_USER_INVALID_ROLE,

        /// <summary>
        /// File related value
        /// </summary>
        File_FILE_NOT_VALID,
        File_FILE_IS_EMPTY,
        File_FILE_TYPE_NOT_ALLOWED,
        File_FILE_SIZE_TOO_LARGE,
        File_FILE_NOT_FOUND,


    }
}