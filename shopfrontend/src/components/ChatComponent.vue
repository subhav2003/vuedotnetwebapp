<template>
  <div class="chat-container bg-white h-full w-full flex flex-col p-4 max-w-2xl mx-auto">
    <h1 class="text-lg sm:text-xl font-bold text-center mb-4">
      Custom Order #{{ props.orderId }}
    </h1>

    <!-- Messages -->
    <div
        ref="chatBox"
        class="flex-1 overflow-y-auto bg-gray-100 rounded-lg px-3 py-4 space-y-1"
    >
      <transition-group name="fade" tag="div">
        <div
            class="py-1"
            v-for="msg in messages"
            :key="msg.id"
            :class="[
            'flex flex-col',
            msg.sender_id === userId ? 'items-end' : 'items-start'
          ]"
        >
          <p
              v-if="msg.sender_id !== userId"
              class="text-xs text-gray-500 mb-0.5 ml-1"
          >
            {{ msg.sender_name }}
          </p>

          <div
              :class="[
              'px-4 py-2 rounded-lg shadow text-sm max-w-[75%] break-words whitespace-pre-line',
              msg.sender_id === userId
                ? 'bg-blue-600 text-white rounded-br-none'
                : 'bg-white text-gray-800 rounded-bl-none'
            ]"
          >
            <p v-if="msg.message" class="leading-snug">{{ msg.message }}</p>
            <img
                v-if="msg.attachment_url"
                :src="msg.attachment_url"
                class="mt-2 rounded-md max-w-full max-h-48 object-contain"
                alt="attachment"
            />
          </div>
        </div>
      </transition-group>
    </div>

    <!-- Input -->
    <form @submit.prevent="sendMessage" class="mt-3 w-full">
      <div class="flex items-center gap-2">
        <input
            v-model="newMessage"
            type="text"
            placeholder="Write a message..."
            class="flex-1 border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring focus:ring-blue-300"
            :disabled="loading"
        />

        <label class="cursor-pointer relative group">
          <input
              type="file"
              accept="image/*"
              @change="handleAttachmentChange"
              class="hidden"
              :disabled="loading"
          />
          <svg
              class="w-6 h-6 text-gray-600 hover:text-blue-500 transition"
              fill="currentColor"
              viewBox="0 0 20 20"
          >
            <path
                d="M4 3a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V5a2 2 0 00-2-2H4zm0 2h12v3l-2.586-2.586a1 1 0 00-1.414 0L9 8.414 7.414 6.828a1 1 0 00-1.414 0L4 9V5zm0 10v-1l3-3 2.586 2.586a1 1 0 001.414 0L16 10v5H4z"
            />
          </svg>
          <span
              class="absolute bottom-full left-1/2 -translate-x-1/2 mb-1 text-xs text-white bg-black px-2 py-1 rounded opacity-0 group-hover:opacity-100 transition"
          >
            Only image files
          </span>
        </label>

        <button
            type="submit"
            :disabled="( !newMessage.trim() && !attachment ) || loading"
            class="bg-blue-600 text-white px-4 py-2 rounded-md text-sm disabled:opacity-50 disabled:cursor-not-allowed hover:bg-blue-700 transition"
        >
          {{ loading ? 'Sending…' : 'Send' }}
        </button>
      </div>

      <p v-if="attachmentName" class="text-xs text-gray-500 mt-1 ml-1">
        📎 {{ attachmentName }}
      </p>
    </form>
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount, nextTick } from 'vue'
import axios from 'axios'
import Pusher from 'pusher-js'

const props = defineProps({ orderId: Number })

const userId = Number(localStorage.getItem('userId'))
const token = localStorage.getItem('authToken')
const apiUrl = process.env.VUE_APP_API_URL || 'http://127.0.0.1:8000'
const pusherAppKey = process.env.VUE_PUSHER_APP_KEY || 'f3e9ed6993f9c51b233c'
const pusherAppCluster = process.env.VUE_PUSHER_APP_CLUSTER || 'ap2'

const messages = ref([])
const newMessage = ref('')
const attachment = ref(null)
const attachmentName = ref('')
const chatBox = ref(null)
const loading = ref(false)   // <-- guard

const scrollToBottom = () => {
  nextTick(() => {
    if (chatBox.value) {
      chatBox.value.scrollTop = chatBox.value.scrollHeight
    }
  })
}

const handleAttachmentChange = (e) => {
  if (loading.value) return
  const file = e.target.files[0]
  attachment.value = file || null
  attachmentName.value = file?.name || ''
}

const fetchMessages = async () => {
  const res = await axios.get(`${apiUrl}/api/v1/orders/${props.orderId}/messages`, {
    headers: { Authorization: `Bearer ${token}` }
  })
  messages.value = res.data
  scrollToBottom()
}

const sendMessage = async () => {
  if (loading.value) return          // <-- prevent re-entry
  if (!newMessage.value.trim() && !attachment.value) return

  loading.value = true               // <-- start
  const formData = new FormData()
  if (newMessage.value.trim()) formData.append('message', newMessage.value)
  if (attachment.value) formData.append('attachment', attachment.value)

  try {
    await axios.post(
        `${apiUrl}/api/v1/orders/${props.orderId}/messages`,
        formData,
        {
          headers: {
            Authorization: `Bearer ${token}`,
            'Content-Type': 'multipart/form-data'
          }
        }
    )
    newMessage.value = ''
    attachment.value = null
    attachmentName.value = ''
    scrollToBottom()
  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false            // <-- end
  }
}

let pusher, channel
onMounted(() => {
  fetchMessages()
  pusher = new Pusher(pusherAppKey, { cluster: pusherAppCluster, forceTLS: true })
  channel = pusher.subscribe(`order.${props.orderId}`)
  channel.bind('message.sent', data => {
    messages.value.push(data.message)
    scrollToBottom()
  })
})

onBeforeUnmount(() => {
  if (channel) {
    channel.unbind_all()
    channel.unsubscribe()
  }
})
</script>

<style scoped>
.fade-enter-active, .fade-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}
.fade-enter-from {
  opacity: 0;
  transform: translateY(4px);
}
.fade-enter-to {
  opacity: 1;
  transform: translateY(0);
}
</style>
