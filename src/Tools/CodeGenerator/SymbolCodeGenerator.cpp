#include "SymbolCodeGenerator.h"
#include <sstream>

#include "Misc.h"

using namespace std;
using namespace std::filesystem;

//void GenerateSymbol(path srcPath)
//{
//    // src/Symbol/Symbols.g.h
//    // src/Symbol/Symbols.g.cpp
//    path hPath = [srcPath]() mutable { return absolute(srcPath.append("..").append("include").append("Symbol").append("Symbols.g.h")); }();
//    path cppPath = [srcPath]() mutable { return srcPath.append("Symbol").append("Symbols.g.cpp"); }();
//
//    ostringstream hStream, cppStream;
//
//    //  
//    hStream << R"---(#pragma once
//#include "SymbolConfig.h"
//#include <string>
//#include <vector>
//#include <optional>
//#include <memory>
//#include <variant>
//
//#include <Infra/Json.h>
//#include <Infra/Unreachable.h>
//
//namespace Citron {
//
//)---";
//
//    cppStream << R"---(#include "pch.h"
//
//#include <Symbol/Symbols.g.h>
//#include <Infra/Json.h>
//
//using namespace std;
//
//namespace Citron {
//
//)---";
//
//    CommonInfo commonInfo = { .linkage = "SYMBOL_API" };
//
//    vector<ItemInfo> itemInfos {
//
//        // DeclSymbols
//        ClassInfo {
//            .name = "ClassDeclSymbol",
//        
//            .memberInfos {
//                
//                { .type = "IDeclSymbolNode", .memberVarName = "outer", .getterName = "GetOuter", .bUsePimpl = false },
//                { .type = "Accessor", .memberVarName = "accessor", .getterName = "GetAccessor", .bUsePimpl = false },
//                { .type = "Name", .memberVarName = "name", .getterName = "GetName", .bUsePimpl = false },
//                { .type = "std::vector<Name>", .memberVarName = "typeParams", .getterName = "GetTypeParams", .bUsePimpl = false },
//                { .type = "std::optional<ClassType>", .memberVarName = "baseClass", .getterName = "GetBaseClass", .bUsePimpl = false },
//                { .type = "std::vector<InterfaceType>", .memberVarName = "interfaces", .getterName = "GetInterfaces", .bUsePimpl = false },
//                { .type = "std::vector<ClassMemberVarDeclSymbol>", .memberVarName = "memberVars", .getterName = "GetMemberVars", .bUsePimpl = false },
//                { .type = "std::vector<ClassConstructorDeclSymbol>", .memberVarName = "constructors", .getterName = "GetConstructors", .bUsePimpl = false },
//                { .type = "std::optional<int>", .memberVarName = "trivialConstructorIndex", .getterName = "GetTrivialConstructorIndex", .bUsePimpl = false },
//                { .type = "std::optional<int>", .memberVarName = "trivialConstructorIndex", .getterName = "GetTrivialConstructorIndex", .bUsePimpl = false },
//
//                
//            }
//
//            .bDefaultsInline = false // T의 크기가 알려져 있을때, pimpl이 아닐때 true를 쓴다
//        },
//    };
//
//    GenerateItems(commonInfo, hStream, cppStream, itemInfos);
//
//    // footer(close namespaces)
//    hStream << endl << '}' << endl;
//    cppStream << '}' << endl;
//
//    WriteAll(hPath, hStream.str());
//    WriteAll(cppPath, cppStream.str());
//}
//
