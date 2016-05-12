﻿////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

namespace Realms
{
/// <summary>
/// Exception thrown when you're trying to use Manage but the object is already managed by a different Realm.
/// </summary>
public class RealmObjectManagedByAnotherRealmException : RealmException
{
    internal RealmObjectManagedByAnotherRealmException(string detailMessage) : base(detailMessage)
    {

    }
}

}
