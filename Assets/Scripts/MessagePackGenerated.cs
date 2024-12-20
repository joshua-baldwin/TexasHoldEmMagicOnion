// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY MPC(MessagePack-CSharp). DO NOT CHANGE IT.
// </auto-generated>

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168
#pragma warning disable CS1591 // document public APIs

#pragma warning disable SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1649 // File name should match first type name

namespace MessagePack.Resolvers
{
    public class GeneratedResolver : global::MessagePack.IFormatterResolver
    {
        public static readonly global::MessagePack.IFormatterResolver Instance = new GeneratedResolver();

        private GeneratedResolver()
        {
        }

        public global::MessagePack.Formatters.IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.Formatter;
        }

        private static class FormatterCache<T>
        {
            internal static readonly global::MessagePack.Formatters.IMessagePackFormatter<T> Formatter;

            static FormatterCache()
            {
                var f = GeneratedResolverGetFormatterHelper.GetFormatter(typeof(T));
                if (f != null)
                {
                    Formatter = (global::MessagePack.Formatters.IMessagePackFormatter<T>)f;
                }
            }
        }
    }

    internal static class GeneratedResolverGetFormatterHelper
    {
        private static readonly global::System.Collections.Generic.Dictionary<global::System.Type, int> lookup;

        static GeneratedResolverGetFormatterHelper()
        {
            lookup = new global::System.Collections.Generic.Dictionary<global::System.Type, int>(7)
            {
                { typeof(global::System.Collections.Generic.List<global::THE.MagicOnion.Shared.Entities.CardEntity>), 0 },
                { typeof(global::THE.MagicOnion.Shared.Entities.CardEntity[]), 1 },
                { typeof(global::THE.MagicOnion.Shared.Entities.CardRankEnum), 2 },
                { typeof(global::THE.MagicOnion.Shared.Entities.CardSuitEnum), 3 },
                { typeof(global::THE.MagicOnion.Shared.Entities.PlayerRoleEnum), 4 },
                { typeof(global::THE.MagicOnion.Shared.Entities.CardEntity), 5 },
                { typeof(global::THE.MagicOnion.Shared.Entities.PlayerEntity), 6 },
            };
        }

        internal static object GetFormatter(global::System.Type t)
        {
            int key;
            if (!lookup.TryGetValue(t, out key))
            {
                return null;
            }

            switch (key)
            {
                case 0: return new global::MessagePack.Formatters.ListFormatter<global::THE.MagicOnion.Shared.Entities.CardEntity>();
                case 1: return new global::MessagePack.Formatters.ArrayFormatter<global::THE.MagicOnion.Shared.Entities.CardEntity>();
                case 2: return new MessagePack.Formatters.THE.MagicOnion.Shared.Entities.CardRankEnumFormatter();
                case 3: return new MessagePack.Formatters.THE.MagicOnion.Shared.Entities.CardSuitEnumFormatter();
                case 4: return new MessagePack.Formatters.THE.MagicOnion.Shared.Entities.PlayerRoleEnumFormatter();
                case 5: return new MessagePack.Formatters.THE.MagicOnion.Shared.Entities.CardEntityFormatter();
                case 6: return new MessagePack.Formatters.THE.MagicOnion.Shared.Entities.PlayerEntityFormatter();
                default: return null;
            }
        }
    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning restore SA1312 // Variable names should begin with lower-case letter
#pragma warning restore SA1649 // File name should match first type name


// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY MPC(MessagePack-CSharp). DO NOT CHANGE IT.
// </auto-generated>

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168
#pragma warning disable CS1591 // document public APIs

#pragma warning disable SA1403 // File may only contain a single namespace
#pragma warning disable SA1649 // File name should match first type name

namespace MessagePack.Formatters.THE.MagicOnion.Shared.Entities
{

    public sealed class CardRankEnumFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::THE.MagicOnion.Shared.Entities.CardRankEnum>
    {
        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::THE.MagicOnion.Shared.Entities.CardRankEnum value, global::MessagePack.MessagePackSerializerOptions options)
        {
            writer.Write((global::System.Int32)value);
        }

        public global::THE.MagicOnion.Shared.Entities.CardRankEnum Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            return (global::THE.MagicOnion.Shared.Entities.CardRankEnum)reader.ReadInt32();
        }
    }

    public sealed class CardSuitEnumFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::THE.MagicOnion.Shared.Entities.CardSuitEnum>
    {
        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::THE.MagicOnion.Shared.Entities.CardSuitEnum value, global::MessagePack.MessagePackSerializerOptions options)
        {
            writer.Write((global::System.Int32)value);
        }

        public global::THE.MagicOnion.Shared.Entities.CardSuitEnum Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            return (global::THE.MagicOnion.Shared.Entities.CardSuitEnum)reader.ReadInt32();
        }
    }

    public sealed class PlayerRoleEnumFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::THE.MagicOnion.Shared.Entities.PlayerRoleEnum>
    {
        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::THE.MagicOnion.Shared.Entities.PlayerRoleEnum value, global::MessagePack.MessagePackSerializerOptions options)
        {
            writer.Write((global::System.Int32)value);
        }

        public global::THE.MagicOnion.Shared.Entities.PlayerRoleEnum Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            return (global::THE.MagicOnion.Shared.Entities.PlayerRoleEnum)reader.ReadInt32();
        }
    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning restore SA1403 // File may only contain a single namespace
#pragma warning restore SA1649 // File name should match first type name



// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY MPC(MessagePack-CSharp). DO NOT CHANGE IT.
// </auto-generated>

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168
#pragma warning disable CS1591 // document public APIs

#pragma warning disable SA1129 // Do not use default value type constructor
#pragma warning disable SA1309 // Field names should not begin with underscore
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1403 // File may only contain a single namespace
#pragma warning disable SA1649 // File name should match first type name

namespace MessagePack.Formatters.THE.MagicOnion.Shared.Entities
{
    public sealed class CardEntityFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::THE.MagicOnion.Shared.Entities.CardEntity>
    {

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::THE.MagicOnion.Shared.Entities.CardEntity value, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            global::MessagePack.IFormatterResolver formatterResolver = options.Resolver;
            writer.WriteArrayHeader(2);
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::THE.MagicOnion.Shared.Entities.CardSuitEnum>(formatterResolver).Serialize(ref writer, value.Suit, options);
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::THE.MagicOnion.Shared.Entities.CardRankEnum>(formatterResolver).Serialize(ref writer, value.Rank, options);
        }

        public global::THE.MagicOnion.Shared.Entities.CardEntity Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            options.Security.DepthStep(ref reader);
            global::MessagePack.IFormatterResolver formatterResolver = options.Resolver;
            var length = reader.ReadArrayHeader();
            var __Suit__ = default(global::THE.MagicOnion.Shared.Entities.CardSuitEnum);
            var __Rank__ = default(global::THE.MagicOnion.Shared.Entities.CardRankEnum);

            for (int i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        __Suit__ = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::THE.MagicOnion.Shared.Entities.CardSuitEnum>(formatterResolver).Deserialize(ref reader, options);
                        break;
                    case 1:
                        __Rank__ = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::THE.MagicOnion.Shared.Entities.CardRankEnum>(formatterResolver).Deserialize(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            var ____result = new global::THE.MagicOnion.Shared.Entities.CardEntity(__Suit__, __Rank__);
            reader.Depth--;
            return ____result;
        }
    }

    public sealed class PlayerEntityFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::THE.MagicOnion.Shared.Entities.PlayerEntity>
    {

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::THE.MagicOnion.Shared.Entities.PlayerEntity value, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            global::MessagePack.IFormatterResolver formatterResolver = options.Resolver;
            writer.WriteArrayHeader(8);
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<string>(formatterResolver).Serialize(ref writer, value.Name, options);
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Guid>(formatterResolver).Serialize(ref writer, value.Id, options);
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::THE.MagicOnion.Shared.Entities.PlayerRoleEnum>(formatterResolver).Serialize(ref writer, value.PlayerRole, options);
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Guid>(formatterResolver).Serialize(ref writer, value.RoomId, options);
            writer.Write(value.IsDealer);
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::THE.MagicOnion.Shared.Entities.CardEntity[]>(formatterResolver).Serialize(ref writer, value.CardHand, options);
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.List<global::THE.MagicOnion.Shared.Entities.CardEntity>>(formatterResolver).Serialize(ref writer, value.CardPool, options);
            writer.Write(value.IsReady);
        }

        public global::THE.MagicOnion.Shared.Entities.PlayerEntity Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            options.Security.DepthStep(ref reader);
            global::MessagePack.IFormatterResolver formatterResolver = options.Resolver;
            var length = reader.ReadArrayHeader();
            var __Name__ = default(string);
            var __Id__ = default(global::System.Guid);
            var __PlayerRole__ = default(global::THE.MagicOnion.Shared.Entities.PlayerRoleEnum);
            var __RoomId__ = default(global::System.Guid);
            var __IsDealer__ = default(bool);
            var __CardHand__ = default(global::THE.MagicOnion.Shared.Entities.CardEntity[]);
            var __CardPool__ = default(global::System.Collections.Generic.List<global::THE.MagicOnion.Shared.Entities.CardEntity>);
            var __IsReady__ = default(bool);

            for (int i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        __Name__ = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<string>(formatterResolver).Deserialize(ref reader, options);
                        break;
                    case 1:
                        __Id__ = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Guid>(formatterResolver).Deserialize(ref reader, options);
                        break;
                    case 2:
                        __PlayerRole__ = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::THE.MagicOnion.Shared.Entities.PlayerRoleEnum>(formatterResolver).Deserialize(ref reader, options);
                        break;
                    case 3:
                        __RoomId__ = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Guid>(formatterResolver).Deserialize(ref reader, options);
                        break;
                    case 4:
                        __IsDealer__ = reader.ReadBoolean();
                        break;
                    case 5:
                        __CardHand__ = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::THE.MagicOnion.Shared.Entities.CardEntity[]>(formatterResolver).Deserialize(ref reader, options);
                        break;
                    case 6:
                        __CardPool__ = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Collections.Generic.List<global::THE.MagicOnion.Shared.Entities.CardEntity>>(formatterResolver).Deserialize(ref reader, options);
                        break;
                    case 7:
                        __IsReady__ = reader.ReadBoolean();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            var ____result = new global::THE.MagicOnion.Shared.Entities.PlayerEntity(__Name__, __Id__, __PlayerRole__);
            if (length <= 3)
            {
                goto MEMBER_ASSIGNMENT_END;
            }

            ____result.RoomId = __RoomId__;
            if (length <= 4)
            {
                goto MEMBER_ASSIGNMENT_END;
            }

            ____result.IsDealer = __IsDealer__;
            if (length <= 5)
            {
                goto MEMBER_ASSIGNMENT_END;
            }

            ____result.CardHand = __CardHand__;
            if (length <= 6)
            {
                goto MEMBER_ASSIGNMENT_END;
            }

            ____result.CardPool = __CardPool__;
            if (length <= 7)
            {
                goto MEMBER_ASSIGNMENT_END;
            }

            ____result.IsReady = __IsReady__;

        MEMBER_ASSIGNMENT_END:
            reader.Depth--;
            return ____result;
        }
    }

}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning restore SA1129 // Do not use default value type constructor
#pragma warning restore SA1309 // Field names should not begin with underscore
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
#pragma warning restore SA1403 // File may only contain a single namespace
#pragma warning restore SA1649 // File name should match first type name

